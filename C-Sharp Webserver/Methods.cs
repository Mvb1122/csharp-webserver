using System.Net;
using WebServer;


namespace Main
{
    internal class Methods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { RegisterPlayer, GetDefaultResponse };

        public static ResponseInformation GetDefaultResponse(HttpListenerRequest req)
        {
            return new ResponseInformation(req, Helpers.GetMime("json"), "{ \"sucessful\": true }");
        }

        public static ResponseInformation RegisterPlayer(HttpListenerRequest req)
        {
            // Select an ID that doesn't already exist.
            FileInfo[] files = GetFilesInDirectory($"{WebServer.WebServer.BasePath}/Players/");
            bool FileListContains(string fileName)
            {
                foreach (FileInfo file in files) if (file.Name == fileName) return false;

                return true;
            }

            int playerID;
            do
            {
                playerID = new Random().Next();
            } while (!FileListContains($"{playerID}.json"));

            // Return the ID, and write the file.
            Dictionary<string, string> response = new()
            {
                { "sucessful", "true" },
                { "id", playerID.ToString() }
            };

            // Write the file.
            string playerPath = $"{WebServer.WebServer.BasePath}/Players/{playerID}.json";
            Dictionary<string, string> PlayerFile = new()
            {
                { "id", playerID.ToString() }
            };
            File.WriteAllText(playerPath, Helpers.DictionaryToJSON(PlayerFile));

            return new ResponseInformation(req, response);
        }

        /// <summary>
        /// Returns the list of files in the directory, or creates it if it doesn't exist.
        /// </summary>
        /// <param name="AbsolutePath">The absolute path to create the directory in.</param>
        /// <returns>A FileInfo[] of all files in the directory.</returns>
        private static FileInfo[] GetFilesInDirectory(string AbsolutePath)
        {
            var directory = new DirectoryInfo(AbsolutePath);
            if (!directory.Exists) directory.Create();
            return directory.GetFiles();
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
            if (string.IsNullOrEmpty(ip) && req.Headers["REMOTE_ADDR"] != null) ip = req.Headers["REMOTE_ADDR"]!.ToString();

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
            // TODO: Ping all servers to request player count information and return servers by player count and cap.
            return new ResponseInformation(req, new ServersListResponse());
        }

        class ServersListResponse
        {
            public string[] Servers { get; set; }
            public ServersListResponse()
            {
                Servers = ServerMethods.Servers.ToArray();
            }
        }
    }
}