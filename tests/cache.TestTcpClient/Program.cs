using System.Net;
using System.Net.Sockets;
using System.Text;

namespace cache.TestTcpClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var address = "127.0.0.1";
            var port = 55555;
            var endpoint = new IPEndPoint(IPAddress.Parse(address), port);

            Socket client = new Socket(endpoint.AddressFamily,
                   SocketType.Stream, ProtocolType.Tcp);

            try
            {

                // Connect Socket to the remote 
                // endpoint using method Connect()
                client.Connect(endpoint);

                // We print EndPoint information 
                // that we are connected
                Console.WriteLine("Socket connected to -> {0} ",
                              client.RemoteEndPoint.ToString());

                while (true)
                {
                    var rawInput = Console.ReadLine();
                    byte[] message = Encoding.ASCII.GetBytes(rawInput+Environment.NewLine);
                    int byteSent = client.Send(message);
                    byte[] messageReceived = new byte[1024];
                    int byteRecv = client.Receive(messageReceived);
                    Console.WriteLine("Message from Server -> {0}",
                        Encoding.ASCII.GetString(messageReceived,
                                                0, byteRecv));
                }
            }

            // Manage of Socket's Exceptions
            catch (ArgumentNullException ane)
            {

                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }

            catch (SocketException se)
            {

                Console.WriteLine("SocketException : {0}", se.ToString());
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
