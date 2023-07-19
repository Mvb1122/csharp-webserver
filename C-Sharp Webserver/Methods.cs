using System.Net;
using System.Text;
using ResponseInformation = WebServer.ResponseInformation;
using Helpers= WebServer.Helpers;
using System.Text.Json;
using System.Text.RegularExpressions;

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

            return new ResponseInformation(req, "application/json", "{ \"successful\": true }");
        }

        public class STDevResponse
        {
            public bool Sucessful { get; set; }
            public double STDev { get; set; }
        }
    }


    internal class ServerMethods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { game_ListIPAsServer, game_UnlistIPAsServer, game_GetServersList, game_GetItemsList, game_GetInventory, game_MakeNewUser, game_UpdateInventory, game_GetLoginToken, game_WriteFile, game_GetPrefab };

        static string DefaultResponse = "{ \"successful\": true }";

        private static readonly List<string> Servers = new List<string>(0);
        public static ResponseInformation game_ListIPAsServer(HttpListenerRequest req)
        {
            // Extract the request's source IP.
            string ip = req.RemoteEndPoint.Address.ToString();
            Console.WriteLine($"New Server's IP: {ip}");

            // Add the IP to the list.
            Servers.Add(ip);

            return new ResponseInformation(req, "application/json", DefaultResponse);
        }

        public static ResponseInformation game_UnlistIPAsServer(HttpListenerRequest req)
        {
            Servers.Remove(req.RemoteEndPoint.Address.ToString());
            return new ResponseInformation(req, "application/json", DefaultResponse);
        }

        public static ResponseInformation game_GetServersList(HttpListenerRequest req)
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
        public static ResponseInformation game_GetItemsList(HttpListenerRequest req)
        {
            return new ResponseInformation(req, ILR);
        }

        // ExampleData: { Token: 1234 } 
        public class TokenReq { public int Token { get; set; } }
        public static ResponseInformation game_GetInventory(HttpListenerRequest req)
        {
            //resp.Add("ReqType", req.HttpMethod);
            if (req.HttpMethod == "POST")
            {
                string body = Helpers.GetRequestPostData(req);
                TokenReq t = JsonSerializer.Deserialize<TokenReq>(body);

                // Read back the user's inventory and send it to them.
                if (t != null && t.Token != 0)
                {
                    string username = LoginTokens[t.Token];
                    Console.WriteLine("Attempting to get the inventory of " +  username);

                    User u = User.MakeUserFromUserName(username);
                    if (u != null)
                    {
                        Inventory i = new Inventory()
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
        public static ResponseInformation game_GetLoginToken(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                LoginReq ob = JsonSerializer.Deserialize<LoginReq>(body);

                // Check the user's information 
                User u = User.MakeUserFromUserName(ob.Username);
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

            return new ResponseInformation(req, false);
        }


        // ExampleData: { Username: "Admin", Password: "Debug" }
        // ExampleReturnedData: { Successful: true, Token: 1234 }
        public static ResponseInformation game_MakeNewUser(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                // First, create a new user from the provided data.
                Console.WriteLine(body);
                LoginReq ob = JsonSerializer.Deserialize<LoginReq>(body);
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
        public static ResponseInformation game_UpdateInventory(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                InventoryUpdateRequest ob = JsonSerializer.Deserialize<InventoryUpdateRequest>(body);

                // Get the user.
                string username = LoginTokens[ob.Token]; 
                User user = User.MakeUserFromUserName(username);
                user.Inventory = ob.Inventory;

                // Push changes to file.
                User.WriteToFile(user);
                return new ResponseInformation(req, true);
            }

            return new ResponseInformation(req, false);
        }

        public class FileRequest { public string Path { get; set; } public string Data { get; set; } }
        public static ResponseInformation game_WriteFile(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                // Console.WriteLine($"Attempting to write a file with {body}");
                FileRequest fr = JsonSerializer.Deserialize<FileRequest>(body);
                if (fr != null)
                {
                    string path = fr.Path;
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

        public static ResponseInformation game_GetPrefab(HttpListenerRequest req)
        {
            string body = Helpers.GetRequestPostData(req);
            if (req.HttpMethod == "POST" && body != null)
            {
                FileRequest fr = JsonSerializer.Deserialize<FileRequest>(body);
                if (fr != null)
                {
                    string path = fr.Path;
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
    }
}