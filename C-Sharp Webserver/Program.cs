using FunctionParser;
using WebServer;

namespace Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ws = new WebServer.WebServer("http://localhost:8080/");

            WebServer.WebServer.AddMethod(Methods.Functions);
            WebServer.WebServer.AddMethod(ServerMethods.Functions);
            WebServer.WebServer.AddMethod(CalculusMethods.Functions);

            ws.Run();
            Console.WriteLine("Press a key to exit.");

            // DEBUG: Test Function parsing.
            Func<float, float> eq = FunctionParser.FunctionParser.ParseFunction("((x+1) + 2) + 3");
            Console.WriteLine(eq(3));

            foreach (float angle in CalculusMethods.CoterminalAngles(90)) Console.WriteLine(angle);

            Console.ReadKey();
            ws.Stop();
        }
    }
}