using Microsoft.Extensions.ObjectPool;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace cache.SimpleStoreSystem
{
    public class TcpServer1
    {
        private CancellationTokenSource _cts;
        private Socket _socket;
        private readonly ObjectPool<Queue<(byte[], int)>> _queuePool;
        private readonly SimpleStore _cache;
        private readonly IPEndPoint _endpoint;
        public TcpServer1(string address, int port, CancellationTokenSource cts)
        {
            _cts = cts;

            var provider = new DefaultObjectPoolProvider();
            var policy = new DefaultPooledObjectPolicy<Queue<(byte[], int)>>();
            _queuePool = provider.Create(policy);
            _cache = new SimpleStore();
            _endpoint = new IPEndPoint(IPAddress.Parse(address), port);
        }

        public async Task StartAsync()
        {
            _socket = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(_endpoint);
            _socket.Listen();

            Console.WriteLine($"Socket Server binded to {_endpoint.Address}:{_endpoint.Port} !");

            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var client = await _socket.AcceptAsync();
                    _ = ProcessClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }


        Memory<byte> telentInitSeq = new Memory<byte>([255,251,31,255,251,32,255,251,24,255,251,39,255,253,1,255,251,3,255,253]);

        Memory<byte> terminalSeq = new Memory<byte>([(byte)'\r',(byte)'\n']);
        private async Task ProcessClientAsync(Socket client)
        {
            var bytesRead = -1;
            var chunks = _queuePool.Get();
            var charBuffer = Array.Empty<char>();

            try
            {
                // читаем куски пока читается из подключения
                while (!_cts.IsCancellationRequested && client.Connected)
                {
                    var seqReaded = false;
                    var payloadLength = 0;
                    while (!seqReaded && !_cts.IsCancellationRequested)
                    {
                        // в принципе, можно сделать буфер больше, но так можно условно промоделировать еще и плохое соединение
                        var buffer = ArrayPool<byte>.Shared.Rent(1024);
                        bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None, _cts.Token);

                        if (bytesRead == 0)
                            break;

                        // skip if telnet init 
                        if (buffer.AsSpan().IndexOf(telentInitSeq.Span) >= 0)
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                            continue;
                        }

                        var terminalPosition = buffer.AsSpan().IndexOf(terminalSeq.Span);

                        // если встретили terminalSeq то конец получения команды
                        if (buffer.AsSpan().IndexOf(terminalSeq.Span) >= 0)
                            seqReaded = true;

                        chunks.Enqueue((buffer, bytesRead));
                        payloadLength += bytesRead;
                    }

                    if (payloadLength == 0)
                        break;

                    var rawPayload = new Memory<byte>(ArrayPool<byte>.Shared.Rent(payloadLength));
                    bytesRead = 0;
                    while (chunks.Count > 0)
                    {
                        var (data, length) = chunks.Dequeue();
                        var emptyIndex = data.AsSpan().IndexOf((byte)0);
                        data.AsSpan().Slice(0, length).CopyTo(rawPayload.Slice(bytesRead).Span);
                        bytesRead += length;
                        ArrayPool<byte>.Shared.Return(data);
                    }
                    // Было бы красиво, чтобы заработало такое, но возможно только для UTF16, насколько я понял.
                    //var readyData = MemoryMarshal.Cast<byte, char>(rawPayload.Span.Slice(0,copied));
                    charBuffer = ArrayPool<char>.Shared.Rent(bytesRead);
                    _ = Encoding.ASCII.GetChars(rawPayload.Span, charBuffer);
                    var readyData = charBuffer.AsMemory().Slice(0, bytesRead).Trim();

                    await ProcessCommand(client, readyData);

                    while (chunks.Count > 0)
                    {
                        var (data, length) = chunks.Dequeue();
                        ArrayPool<byte>.Shared.Return(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            finally
            {
                if (charBuffer != Array.Empty<char>())
                {
                    ArrayPool<char>.Shared.Return(charBuffer);
                }
                while (chunks.Count > 0)
                {
                    var (data, length) = chunks.Dequeue();
                    ArrayPool<byte>.Shared.Return(data);
                }
                _queuePool.Return(chunks);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                client.Dispose();
            }
        }
        private byte[] GetReponse(string value)
        {
            return Encoding.ASCII.GetBytes(value + Environment.NewLine);
        }
        private async Task ProcessCommand(Socket client, Memory<char> input)
        {
            var command = CommandParser.Parse(input.Span, ' ');
            var response = Array.Empty<byte>();

            switch (command.CommandType)
            {
                case CommandTypes.Add:
                    {
                        var value = MemoryMarshal.Cast<char, byte>(command.Value);
                        var key = command.Key.ToString();
                        _cache.Set(key, value.ToArray());
                        response = GetReponse($"Key {key} successfuly added");
                        break;
                    }
                case CommandTypes.Get:
                    {
                        var key = command.Key.ToString();
                        var value = _cache.Get(key);
                        response = value;
                        break;
                    }
                case CommandTypes.Delete:
                    {
                        var key = command.Key.ToString();
                        _cache.Delete(key);
                        response = GetReponse($"KEY-VALUE with key {key} successfuly deleted!");
                        break;
                    }
                case CommandTypes.Statistics:
                    {
                        var statistics = _cache.GetStatistics();
                        response = GetReponse($"ADD CALLS: {statistics.SetCount};\nGET CALLS: {statistics.GetCount}\nDELETE CALLS: {statistics.DeleteCount}");
                        break;
                    }
                default:
                    response = GetReponse($"Unknown or incorrect command!");
                    break; 
            }


            await client.SendAsync(response);
        }

        public void Stop()
        {
            _cts.Cancel();
        }
    }
}
