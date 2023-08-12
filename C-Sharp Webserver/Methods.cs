// Ignore Spelling: req Unlist ILR Foldered

using System.Net;
using System.Text;
using ResponseInformation = WebServer.ResponseInformation;
using Helpers= WebServer.Helpers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace Main
{
    internal class Methods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { ExampleRequest, v1_FolderedMethod, RandomNumber, STDev };

        public static ResponseInformation ExampleRequest(HttpListenerRequest? request)
        {
            if (request != null && request.Url != null)
            {
                ResponseInformation? response = new(request, Helpers.GetMime(".txt"), $"Reached! Your URL is: {request.Url.LocalPath}");
                return response;
            }
            else
            {
                return new ResponseInformation(request, false);
            }
        }

        private class RandResponse
        {
            public int Num { get; set; }

            public RandResponse()
            {
                Num = new Random().Next(int.MaxValue);
            }
        }

        public static ResponseInformation RandomNumber(HttpListenerRequest request)
        {
            RandResponse r = new();
            return new ResponseInformation(request, Helpers.GetMime(".json"), Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(r)));
        }

        public static ResponseInformation v1_FolderedMethod(HttpListenerRequest req)
        {
            ResponseInformation response = new(req, "text/plain", $"Reached!");
            return response;
        }

        public static ResponseInformation STDev(HttpListenerRequest req)
        {
            /*
            var response = new STDevResponse()
            {
                Successful = false,
                STDev = 0.0
            };
            */

            return new ResponseInformation(req, "application/json", "{ \"successful\": true }");
        }

        public class STDevResponse
        {
            public bool Successful { get; set; }
            public double STDev { get; set; }
        }
    }


    internal class ServerMethods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions =
        {
            MEngines_ListIPAsServer,
            MEngines_UnlistIPAsServer,
            MEngines_GetServersList,
            MEngines_GetItemsList,
            MEngines_GetInventory,
            MEngines_MakeNewUser,
            MEngines_UpdateInventory,
            MEngines_GetLoginToken,
            MEngines_WriteFile,
            MEngines_GetPrefab,
            MEngines_GetCities
        };

        static readonly string DefaultResponse = "{ \"successful\": true }";
        static readonly string PrefabPath = WebServer.WebServer._basePath + "\\Prefabs\\";

        private static readonly List<string> Servers = new(0);
        public static ResponseInformation MEngines_ListIPAsServer(HttpListenerRequest req)
        {
            // Extract the request's source IP.
            string ip = req.RemoteEndPoint.Address.ToString();
            Console.WriteLine($"New Server's IP: {ip}");

            // Add the IP to the list.
            Servers.Add(ip);

            return new ResponseInformation(req, "application/json", DefaultResponse);
        }

        public static ResponseInformation MEngines_UnlistIPAsServer(HttpListenerRequest req)
        {
            Servers.Remove(req.RemoteEndPoint.Address.ToString());
            return new ResponseInformation(req, "application/json", DefaultResponse);
        }

        public static ResponseInformation MEngines_GetServersList(HttpListenerRequest req)
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

        public static ItemsListResponse ILR = new();
        public static ResponseInformation MEngines_GetItemsList(HttpListenerRequest req)
        {
            return new ResponseInformation(req, ILR);
        }

        // ExampleData: { Token: 1234 } 
        public class TokenReq { public int Token { get; set; } }
        public static ResponseInformation MEngines_GetInventory(HttpListenerRequest req)
        {
            //resp.Add("ReqType", req.HttpMethod);
            if (req.HttpMethod == "POST")
            {
                string body = Helpers.GetRequestPostData(req);
                TokenReq? t = JsonSerializer.Deserialize<TokenReq>(body);

                // Read back the user's inventory and send it to them.
                if (t != null && t.Token != 0)
                {
                    string username = LoginTokens[t.Token];
                    Console.WriteLine("Attempting to get the inventory of " +  username);

                    User? u = User.MakeUserFromUserName(username);
                    if (u != null)
                    {
                        Inventory i = new()
                        {
                            Items = u.Inventory
                        };

                        return new ResponseInformation(req, i);
                    }
                    else return new ResponseInformation(req, false);
                }
            }
            
            return new ResponseInformation(req, false);
        }

        static readonly Dictionary<int, string> LoginTokens = new();

        class LoginReq { public string Username { get; set; } public string Password { get; set; } }
        class TokenResponse { public int token { get; set; } public bool successful { get; set; } }
        // ExampleData: { Username: "Admin", Password: "Debug" }
        // ExampleReturnedData: { Successful: true, Token: 1234 }
        public static ResponseInformation MEngines_GetLoginToken(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                LoginReq? ob = JsonSerializer.Deserialize<LoginReq>(body);

                // Check the user's information 
                if (ob != null)
                {
                    User? u = User.MakeUserFromUserName(ob.Username);
                    if (u != null)
                    {
                        if (u.Password == ob.Password)
                        {
                            // Create a token association with this user's username and return it.
                            int token = (int)Math.Floor(new Random().NextDouble() * 100000);
                            LoginTokens.Add(token, u.Username);
                            TokenResponse resp = new() { 
                                token = token,
                                successful = true,
                            };
                            return new ResponseInformation(req, resp);
                        }
                    }
                }
            }

            return new ResponseInformation(req, false);
        }


        // ExampleData: { Username: "Admin", Password: "Debug" }
        // ExampleReturnedData: { Successful: true, Token: 1234 }
        public static ResponseInformation MEngines_MakeNewUser(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                // First, create a new user from the provided data.
                Console.WriteLine(body);
                LoginReq? ob = JsonSerializer.Deserialize<LoginReq>(body);
                if (ob != null)
                {
                    User.MakeNewUser(ob.Username, ob.Password);
                    // Create a token association with this user's username and return it.
                    int token = (int)Math.Floor(new Random().NextDouble() * 100000);
                    LoginTokens.Add(token, ob.Username);
                    TokenResponse resp = new()
                    {
                        token = token,
                        successful = true,
                    };
                    return new ResponseInformation(req, resp);
                }
            }

            return new ResponseInformation(req, false);
        }

        public class InventoryUpdateRequest { public int Token { get; set; } public ItemCount[] Inventory { get; set; } };

        // **Replaces** whole inventory content!!!
        // ExampleData: { Token: 1234, Inventory: [{ Item: Coal, Count: 123}, etc...] }
        // ExampleReturnedData: { Successful: true, Token: 1234 }
        public static ResponseInformation MEngines_UpdateInventory(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                InventoryUpdateRequest? ob = JsonSerializer.Deserialize<InventoryUpdateRequest>(body);

                // Get the user.
                if (ob != null)
                {
                    string username = LoginTokens[ob.Token]; 
                    User? user = User.MakeUserFromUserName(username);
                    if (user != null)
                    {
                        user.Inventory = ob.Inventory;

                        // Push changes to file.
                        user.WriteToFile();

                        return new ResponseInformation(req, true);
                    }
                }
            }

            return new ResponseInformation(req, false);
        }

        public class FileRequest { public string Path { get; set; } public string Data { get; set; } }
        public static ResponseInformation MEngines_WriteFile(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                // Console.WriteLine($"Attempting to write a file with {body}");
                FileRequest? fr = JsonSerializer.Deserialize<FileRequest>(body);
                if (fr != null)
                {
                    string path = PrefabPath + fr.Path;
                    string data = fr.Data;
                    if (path != null &&  data != null)
                    {
                        // Write the data.
                        File.WriteAllText(path, data);
                        return new ResponseInformation(req, true);
                    }
                }
            }

            return new ResponseInformation(req, false);
        }

        public static ResponseInformation MEngines_GetPrefab(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                FileRequest? fr = JsonSerializer.Deserialize<FileRequest>(body);
                if (fr != null)
                {
                    string path = PrefabPath + fr.Path;
                    if (path != null)
                    {
                        // Read the data.
                        string data = Regex.Unescape(File.ReadAllText(path));
                        return new ResponseInformation(req, Helpers.GetMime(path), data);
                    }
                }
            }

            return new ResponseInformation(req, false);
        }

        public class PrefabList { public CityInWorld[] Prefabs { get; set; } }
        public class CityInWorld
        {
            public string Path {get; set;}
            public float[] Location { get; set;}
            public bool HostedAlready { get; set;}
        }
        public static ResponseInformation MEngines_GetCities(HttpListenerRequest req)
        {
            // Search for files which include City in the name of their prefab 
            List<CityInWorld> cities = new();
            Console.WriteLine(PrefabPath);
            foreach (string f in Directory.GetFiles(PrefabPath)) 
                // If this file does contain city, then add it to the list, minus the path (just send back the file name.)
                if (f.Contains("City"))
                {
                    CityInWorld c = new CityInWorld()
                    {
                        Path = f[(f.LastIndexOf('\\') + 1)..],
                        HostedAlready = false,
                        Location = GetRootLocation(f)
                    };
                    cities.Add(c);
                }

            return new ResponseInformation(req, new PrefabList() { Prefabs = cities.ToArray() });
        }

        private static float[]? GetRootLocation(string Path)
        {
            // Read in the selected city.
            CPOProxy proxy = CPOProxy.CPOFrom(Path);
            if (proxy == null) return null;
            return new float[3] { proxy.Transform.PX, proxy.Transform.PY, proxy.Transform.PZ };
        }
    }


    public class CPOProxy
    {
        private string _name;
        public string Name
        {
            get
            { return _name; }
            set
            {
                _name = value;
            }
        }

        public Transform Transform { get; set; }
        
        public CPOProxy[] Children { get; set; }

        private string path;
        public static CPOProxy? CPOFrom(string path)
        {
            if (File.Exists(path))
            {
                
                CPOProxy? CPO = JsonSerializer.Deserialize<CPOProxy>(File.ReadAllText(path));
                if (CPO != null)
                    CPO.path = path;
                return CPO;
            }
            else return null;
        }

        public void Write() => Write(path);
        public void Write(string path)
        {
            string CPO = JsonSerializer.Serialize(this);
            File.WriteAllTextAsync(path, CPO);
        }
    }
    public class Transform
    {
        public float _PX, _PY, _PZ, _RX, _RY, _RZ, _SX, _SY, _SZ;
        public float PX { get { return _PX; } set { _PX = value; } }
        public float PY { get { return _PY; } set { _PY = value; } }
        public float PZ { get { return _PZ; } set { _PZ = value; } }
        public float RX { get { return _RX; } set { _RX = value; } }
        public float RZ { get { return _RY; } set { _RY = value; } }
        public float SX { get { return _SX; } set { _SX = value; } }
        public float SY { get { return _SY; } set { _SY = value; } }
        public float SZ { get { return _SZ; } set { _SZ = value; } }
    }
}