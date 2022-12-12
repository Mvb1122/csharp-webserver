using System.Net;
using System.Text;
using ResponseInformation = WebServer.ResponseInformation;


namespace Main
{
    internal class Methods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { ExampleRequest, v1_FolderedMethod, RandomNumber };

        public static ResponseInformation ExampleRequest(HttpListenerRequest request)
        {
            ResponseInformation response = new(request, WebServer.Helpers.GetMime(".txt"), $"Reached! Your URL is: {request.Url.LocalPath}");
            return response;
        }

        private class RandResponse
        {
            public int num { get; set; }

            public RandResponse()
            {
                num = new Random().Next(int.MaxValue);
            }
        }

        public static ResponseInformation RandomNumber(HttpListenerRequest request)
        {
            RandResponse r = new RandResponse();
            return new ResponseInformation(request, WebServer.Helpers.GetMime(".json"), Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(r)));
        }

        public static ResponseInformation v1_FolderedMethod(HttpListenerRequest req)
        {
            ResponseInformation response = new(req, "text/plain", $"Reached!");
            return response;
        }

        public async static ResponseInformation STDev(HttpListenerRequest req)
        {
            // Get the list of numbers.
            List<double> numbers = new List<double>(0);
            byte[] body = Array.Empty<byte>();
            await req.InputStream.ReadAsync(body);

            return null;
        }
    }
}