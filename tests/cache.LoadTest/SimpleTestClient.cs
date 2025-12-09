using cache.SimpleStoreSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cache.LoadTest
{
    internal class SimpleTestClient : IDisposable
    {
        private readonly Socket _client;
        private readonly IPEndPoint _endpoint;
        public SimpleTestClient(string address, int port)
        {

            _endpoint = new IPEndPoint(IPAddress.Parse(address), port);
            _client = new Socket(_endpoint.AddressFamily,
                  SocketType.Stream, ProtocolType.Tcp);
        }
        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(_endpoint);
        }
        public async Task<string> SetAsync(string key, UserProfile userProfile)
        {
            var rawInput = $"ADD {key} {JsonSerializer.Serialize(userProfile)}";

            byte[] message = Encoding.ASCII.GetBytes(rawInput + Environment.NewLine);
            await _client.SendAsync(message);

            var response = await GetAnswer();

            return response;
        }
        public async Task<UserProfile> GetAsync(string key)
        {
            var rawInput = $"GET {key}";

            byte[] message = Encoding.ASCII.GetBytes(rawInput + Environment.NewLine);
            await _client.SendAsync(message);


            var response = await GetAnswer();
            try
            {
                var userProfile = JsonSerializer.Deserialize<UserProfile>(response);
                return userProfile;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private async Task<string> GetAnswer()
        {
            byte[] messageReceived = new byte[1024*3];
            int byteRecv = await _client.ReceiveAsync(messageReceived);
            return Encoding.ASCII.GetString(messageReceived,0, byteRecv);
        }

        public void Dispose()
        {
            //_client.Shutdown(SocketShutdown.Both);
            _client.Close();
            _client?.Dispose();
        }
    }
}
