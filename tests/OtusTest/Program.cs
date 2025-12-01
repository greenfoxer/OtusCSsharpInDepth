using cache.SimpleStoreSystem;
using System.IO.Pipelines;

namespace OtusTest
{
    internal class Program
    {
        static async Task Do()
        {
            var pipe = new Pipe();
            var writer = pipe.Writer;
            var reader = pipe.Reader;

            var file = File.OpenRead("d:\\Study\\books\\dotnet\\design-patterns-ru.pdf");

            var t = writer.GetSpan();

            file.ReadExactly(t);
            await writer.FlushAsync();

            try
            {
                var s = await reader.ReadAsync();

            }
            catch (Exception e)
            {

               
            }
            while(true) 
                Thread.Sleep(1000);
        }
        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var tcpServer = new TcpServer1("127.0.0.1", 55555, cts);
            await tcpServer.StartAsync();

        }
        internal struct Test
        {
            public int x; public int y;
        }
    }
}
