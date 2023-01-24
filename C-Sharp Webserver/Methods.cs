using System.Net;
using System.Text;
using ResponseInformation = WebServer.ResponseInformation;
using Helpers= WebServer.Helpers;


namespace Main
{
    internal class Methods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { ExampleRequest, v1_FolderedMethod, RandomNumber, STDev };

        public static ResponseInformation ExampleRequest(HttpListenerRequest request)
        {
            ResponseInformation response = new(request, Helpers.GetMime(".txt"), $"Reached! Your URL is: {request.Url.LocalPath}");
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
            return new ResponseInformation(request, Helpers.GetMime(".json"), Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(r)));
        }

        public static ResponseInformation v1_FolderedMethod(HttpListenerRequest req)
        {
            ResponseInformation response = new(req, "text/plain", $"Reached!");
            return response;
        }

        public static ResponseInformation STDev(HttpListenerRequest req)
        {
            var response = new STDevResponse()
            {
                Sucessful = false,
                STDev = 0.0
            };

            return new ResponseInformation(req, "application/json", "{ \"sucessful\": true }");
        }

        public class STDevResponse
        {
            public bool Sucessful { get; set; }
            public double STDev { get; set; }
        }
    }


    internal class ServerMethods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { Game_ListIPAsServer, Game_UnlistIPAsServer, Game_GetServersList };

        static string DefaultResponse = "{ \"sucessful\": true }";

        private static readonly List<string> Servers = new List<string>(0);
        public static ResponseInformation Game_ListIPAsServer(HttpListenerRequest req)
        {
            // Extract the request's source IP.
            string ip = req.RemoteEndPoint.Address.ToString();
            Console.WriteLine($"New Server's IP: {ip}");

            // Add the IP to the list.
            Servers.Add(ip);

            return new ResponseInformation(req, "application/json", DefaultResponse);
        }

        public static ResponseInformation Game_UnlistIPAsServer(HttpListenerRequest req)
        {
            Servers.Remove(req.RemoteEndPoint.Address.ToString());
            return new ResponseInformation(req, "application/json", DefaultResponse);
        }

        public static ResponseInformation Game_GetServersList(HttpListenerRequest req)
        {
            return new ResponseInformation(req, new ServersListResponse());
        }

        class ServersListResponse {
            public string[] Servers { get; set; }
            public ServersListResponse()
            {
                Servers = ServerMethods.Servers.ToArray();
            }
        }
    }
}