using cache.SimpleStoreSystem;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace cache.TestTcpClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var address = "127.0.0.1";
            var port = 55555;
            var endpoint = new IPEndPoint(IPAddress.Parse(address), port);

            int addId = 1;

            Socket client = new Socket(endpoint.AddressFamily,
                   SocketType.Stream, ProtocolType.Tcp);

            try
            {
                client.Connect(endpoint);
                Console.WriteLine("Socket connected to -> {0} ", client.RemoteEndPoint.ToString());

                while (true)
                {
                    var rawInput = Console.ReadLine();
                    if (rawInput.StartsWith("!SEND"))
                    {
                        var obj = new UserProfile() { Id = addId, CreatedAt=DateTime.Now, UserName = $"Test Name {addId}" };
                        rawInput = $"ADD user:{addId} {JsonSerializer.Serialize(obj)}";

                        addId++;
                    }
                    byte[] message = Encoding.ASCII.GetBytes(rawInput + Environment.NewLine);
                    int byteSent = client.Send(message);
                    byte[] messageReceived = new byte[1024];
                    int byteRecv = client.Receive(messageReceived);
                    Console.WriteLine("Message from Server -> {0}",
                        Encoding.ASCII.GetString(messageReceived,
                                                0, byteRecv));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
            finally
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }
    }
}
