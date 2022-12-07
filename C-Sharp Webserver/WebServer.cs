﻿using System.Net;
using System.Text;

namespace WebServer
{
    using System.IO;
    using System.Reflection;

    public class WebServer
    {
        static Func<HttpListenerRequest, byte[]>[] methods = new Func<HttpListenerRequest, byte[]>[0];
        static Func<HttpListenerRequest, string>[] stringMethods = new Func<HttpListenerRequest, string>[0];

        static readonly string APIPrefix = "/api/";

        public static void AddMethod(Func<HttpListenerRequest, string> method)
        {
            stringMethods = stringMethods.Append(method).ToArray();
        }

        public static void AddMethod(Func<HttpListenerRequest, byte[]> method)
        {
            methods = methods.Append(method).ToArray();
        }

        public static void AddMethod(Func<HttpListenerRequest, string>[] methods)
        {
            foreach (var method in methods) AddMethod(method);
        }

        public static void AddMethod(Func<HttpListenerRequest, byte[]>[] methods)
        {
            foreach (var method in methods) AddMethod(method);
        }

        public static void ListMethods()
        {
            string output = "Methods: \n\t";
            foreach (var method in methods)
                output += APIPrefix + method.Method.Name.Replace("_", "/") + "/" + "\n\t";

            foreach (var method in stringMethods)
                output += APIPrefix + method.Method.Name.Replace("_", "/") + "/" + "\n\t";

            Console.WriteLine(output);
        }

        public static byte[] SendResponse(HttpListenerRequest request)
        {
            if (request == null) return Array.Empty<byte>();

            // Go through provided methods and, if the method name matches that of the request, return its value.
            if (request.Url.LocalPath.IndexOf(APIPrefix) != -1)
            {
                foreach (Func<HttpListenerRequest, byte[]> method in methods)
                {
                    // Determine what path would request this module.
                    string ModulePath = APIPrefix + method.Method.Name.Replace("_", "/") + "/";
                    Console.WriteLine($"Ideal Module Path: {ModulePath}\nActual path: {request.Url.LocalPath}");
                    if (request.Url.LocalPath.Equals(ModulePath))
                    {
                        return method(request);
                    }
                }

                // Do the same for the string methods.
                foreach (var method in stringMethods)
                {
                    string ModulePath = APIPrefix + method.Method.Name.Replace("_", "/") + "/";
                    Console.WriteLine($"Ideal Module Path: {ModulePath}\nActual path: {request.Url.LocalPath}");
                    if (request.Url.LocalPath.Equals(ModulePath))
                    {
                        return Encoding.UTF8.GetBytes(method(request));
                    }
                }
            }

            // If there was no matching method, assume that this was a content request and read the requested data.
            string filePath = request.Url.LocalPath;

            if (filePath.EndsWith('/')) filePath += "index.html";

            if (File.Exists(_basePath + filePath))
            {
                Console.WriteLine($"Trying to read: {_basePath + filePath}");
                return File.ReadAllBytes(_basePath + filePath);
            }
            else return Encoding.UTF8.GetBytes("File not found!");

        }

        private readonly HttpListener requestListener = new HttpListener();
        private readonly Func<HttpListenerRequest, byte[]> _responderMethod;
        private static string _basePath = "./";

        public WebServer(IReadOnlyCollection<string> prefixes, Func<HttpListenerRequest, byte[]> method)
        {
            // URI prefixes are required eg: "http://localhost:8080/test/"
            if (prefixes == null || prefixes.Count == 0)
            {
                throw new ArgumentException("URI prefixes are required");
            }

            if (method == null)
            {
                throw new ArgumentException("responder method required");
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

        public void Run()
        {
            ListMethods();
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
                            HttpListenerContext? ctx = c as HttpListenerContext;
                            try
                            {
                                if (ctx == null)
                                {
                                    return;
                                }

                                var rstr = _responderMethod(ctx.Request);
                                ctx.Response.ContentLength64 = rstr.Length;
                                ctx.Response.OutputStream.Write(rstr, 0, rstr.Length);
                            }
                            catch
                            {
                                // An exception here is ignored because it means there was an issue turning the request data into bytes. 
                            }
                            finally
                            {
                                // Always close the stream, regarless of whether or not it was sucessful.
                                if (ctx != null)
                                {
                                    ctx.Response.OutputStream.Close();
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