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
        static void Main(string[] args)
        {
            //var t1 = CommandParser.Parse("set 1234 12 abcdnhfyrtio".AsSpan());

            //var t2 = CommandParser.Parse("set 1234 5 abcdnhfyrtio".AsSpan());

            //Do();


            var t3 = CommandParser.Parse("del  1234".AsSpan(), ' ');

            var t4 = CommandParser.Parse("get 1234 asdfasdfsd".AsSpan(), ' ');



        }
        internal struct Test
        {
            public int x; public int y;
        }
    }
}
