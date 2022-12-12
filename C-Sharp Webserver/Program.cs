namespace Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ws = new WebServer.WebServer("http://localhost:8080/");

            WebServer.WebServer.AddMethod(Methods.Functions);

            ws.Run();
            Console.WriteLine("Press a key to exit.");
            Console.ReadKey();
            ws.Stop();
        }
    }
}