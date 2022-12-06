using System.Net;
using System.Text;

namespace Main
{
    internal class Methods
    {
        public static string ExampleRequest(HttpListenerRequest request)
        {
            return $"Reached! Your url is: {request.Url.LocalPath}";
        }

        private class RandResponse
        {
            public int num { get; set; }

            public RandResponse()
            {
                num = new Random().Next(int.MaxValue);
            }
        }

        public static byte[] RandomNumber(HttpListenerRequest request)
        {
            RandResponse r = new RandResponse();
            return Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(r));
        }
    }
}