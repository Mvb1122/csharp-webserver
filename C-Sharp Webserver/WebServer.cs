﻿#define debug

using System.Net;
using System.Text;

namespace WebServer
{
    using System.IO;
    using System.Reflection;
    public class ResponseInformation
    {
        public HttpListenerRequest request;
        public bool isDataString;
        public string? dataString;
        public byte[]? _data;
        public byte[]? data
        {
            get
            {
                if (isDataString) return Encoding.UTF8.GetBytes(dataString);
                else return _data;
            }
        }
        public string contentType;

        public ResponseInformation(HttpListenerRequest request, string contentType, byte[] data)
        {
            this.request = request;
            this.contentType = contentType;
            isDataString = false;
            _data = data;
        }

        public ResponseInformation(HttpListenerRequest request, string contentType, string dataString)
        {
            this.request = request;
            this.isDataString = true;
            this.dataString = dataString;
            this.contentType = contentType;
        }

        public ResponseInformation(HttpListenerRequest request, object JSONData)
        {
            this.request = request;
            this.isDataString = true;
            this.contentType = Helpers.GetMime(".json");
            this.dataString = System.Text.Json.JsonSerializer.Serialize(JSONData);
        }

        public ResponseInformation(HttpListenerRequest request, bool IsSuccessful) : this(request, Helpers.GetMime(".json"), "{ \"successful\": " + IsSuccessful.ToString().ToLower() + " }")
        {
            
        }
    }

    public class WebServer
    {
        static Func<HttpListenerRequest, ResponseInformation>[] methods = new Func<HttpListenerRequest, ResponseInformation>[0];

        // Create a dictionary of methods for increased performance.
        static Dictionary<string, Func<HttpListenerRequest, ResponseInformation>> combinedMethods = new Dictionary<string, Func<HttpListenerRequest, ResponseInformation>>();

        static readonly string APIPrefix = "/api/";

        public static void AddMethod(Func<HttpListenerRequest, ResponseInformation> method)
        {
            methods = methods.Append(method).ToArray();
        }

        public static void AddMethod(Func<HttpListenerRequest, ResponseInformation>[] methods)
        {
            foreach (var method in methods) AddMethod(method);
        }

        /// <summary>
        /// Adds the methods to the dictionary and also logs them.
        /// </summary>
        public static void ListMethodsAndAddToDictionary()
        {
            string output = "Methods: \n\t";
            foreach (var method in methods)
            {
                string name = APIPrefix + method.Method.Name.Replace("_", "/") + "/";
                output += name + "\n\t";
                combinedMethods.Add(name, method);
            }

            Console.WriteLine(output);
        }

        public static byte[] SendResponse(HttpListenerRequest request)
        {
            // By default, return JSON as the content type. (It's expected that a module should return JSON.)
            string contentType = "text/plain";
            if (request == null) return Encoding.UTF8.GetBytes("No request! 404");

            // Go through provided methods and, if the method name matches that of the request, return its value.
            if (request.Url.LocalPath.IndexOf(APIPrefix) != -1)
            {
                // Extract module path from string, look it up and run it, if it exists.
                Func<HttpListenerRequest, ResponseInformation> function = combinedMethods[request.Url.LocalPath];
                var result = function(request);

#if debug
                /*
                if (request.HttpMethod == "POST")
                    Console.WriteLine($"Data from client: {Helpers.GetRequestPostData(request)}");
                */
                Console.WriteLine($"Response from {request.Url.LocalPath}: {result.data}");
#endif

                return result.data;
            }

            // If there was no matching method, assume that this was a content request and read the requested data.
            string filePath = request.Url.LocalPath;

            if (filePath.EndsWith('/')) filePath += "index.html";

            if (File.Exists(_basePath + filePath))
            {
#if debug
                Console.WriteLine($"Trying to read: {_basePath + filePath}");
#endif
                // Set the content type correctly.
                contentType = Helpers.GetMime(filePath);
                return File.ReadAllBytes(_basePath + filePath);
            }
            else return Encoding.UTF8.GetBytes("File not found!");
        }

        private readonly HttpListener requestListener = new HttpListener();
        private readonly Func<HttpListenerRequest, byte[]> _responderMethod;
        public static string _basePath = "./";

        public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, byte[]> method)
        {
            // URI prefixes are required eg: "http://localhost:8080/test/"
            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("URI prefixes are required");
            }

            if (method == null)
            {
                throw new ArgumentException("Responder method required");
            }

            foreach (var s in prefixes)
            {
                requestListener.Prefixes.Add(s);
            }

            _responderMethod = method;
            requestListener.Start();

            // Determine where the server is running.
            _basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Console.WriteLine($"Server running at: {_basePath}");
        }

        public WebServer(Func<HttpListenerRequest, byte[]> method, params string[] prefixes) : this(prefixes, method)
        {
        }

        public WebServer(string prefixes) : this(SendResponse, prefixes)
        {
            Console.WriteLine("Initializing web server with default response method!");
        }

        public WebServer(string[] prefixes) : this(SendResponse, prefixes)
        {
            Console.WriteLine("Initializing web server with default response method!");
        }

        public void Run()
        {
            ListMethodsAndAddToDictionary();
            ThreadPool.QueueUserWorkItem(o =>
            {
                Console.WriteLine("Webserver running!");
                try
                {
                    while (requestListener.IsListening)
                    {
                        // Use individual threads for each request, so that one call can't block the whole server. 
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            HttpListenerContext? Request = c as HttpListenerContext;
                            if (Request == null)
                            {
                                return;
                            }

                            try
                            {

                                var APIReturnedValue = _responderMethod(Request.Request);
                                Request.Response.ContentLength64 = APIReturnedValue.Length;
                                Request.Response.OutputStream.Write(APIReturnedValue, 0, APIReturnedValue.Length);
                            }
                            catch (Exception e)
                            {
                                // An exception here means there was an issue turning the request data into bytes, so state as such.
                                var APIReturnedValue = Encoding.UTF8.GetBytes($"There was an error running this request! Error:\n{e}");
                                Console.WriteLine($"Error in request!\n{e}");
                                Request.Response.ContentLength64 = APIReturnedValue.Length;
                                Request.Response.OutputStream.Write(APIReturnedValue, 0, APIReturnedValue.Length);
                            }
                            finally
                            {
                                // Always close the stream, regardless of whether or not it was successful.
                                if (Request != null)
                                {
                                    Request.Response.OutputStream.Close();
                                }
                            }
                        }, requestListener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    // Ignore an exception here, as it's likely the result of a malformed request. 
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public void Stop()
        {
            requestListener.Stop();
            requestListener.Close();
        }
    }
}