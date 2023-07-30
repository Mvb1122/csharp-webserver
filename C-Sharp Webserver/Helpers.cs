namespace WebServer
{
    using System.Text.Json;
    using System.IO;
    using System.Net;
    using Main;
    using System;
    using System.Diagnostics;

    public class Helpers
    {
        public static string ObjectToJSON(object o)
        {
            return JsonSerializer.Serialize(o);
        }

        private static readonly Dictionary<string, string> _mimeTypes = new()
        {
            {".txt", "text/plain"},
            {".html", "text/html"},
            {".gif", "image/gif"},
            {".ico", "image/x-icon"},
            {".json", "application/json"},
            {".mp3", "audio/mpeg"},
            {".mp4", "video/mp4"},
            {".jpeg", "image/jpeg"},
            {".jpg", "image/jpeg"},
            {".png", "image/png"},
            {".pdf", "application/pdf"},
            {".svg", "image/svg+xml"},
            {".wav", "audio/wav"},
            {".css", "text/css"},
            {".php", "text/html"}
        };

        /// <summary>
        /// Returns the MIME type of the file type specified.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetMime(string filePath)
        {
            // Get the file extension from the file path
            string fileExtension = Path.GetExtension(filePath);

            // Check if the file extension is valid and supported
            if (string.IsNullOrEmpty(fileExtension) || !_mimeTypes.ContainsKey(fileExtension))
            {
                // If the file extension is not valid or supported, return "application/octet-stream" as the default MIME type
                return "application/octet-stream";
            }

            // If the file extension is valid and supported, return the corresponding MIME type from the dictionary
            return _mimeTypes[fileExtension];
        }

        /// <param name="request">The request to be parsed.</param>
        /// <returns>Returns the body of the post request passed, or null if no data.</returns>
        public static string GetRequestPostData(HttpListenerRequest request)
        {
            if (!request.HasEntityBody)
            {
                return null;
            }
            using System.IO.Stream body = request.InputStream; // here we have data
            using var reader = new System.IO.StreamReader(body, request.ContentEncoding);
            return reader.ReadToEnd();
        }

        public static T? ReadJSONFileToObject<T>(string path)
        {
            // Read file to string. 
            // DEBUG: Write out character codes.
            // foreach (char c in path.ToCharArray()) Console.WriteLine($"Character: {c}, Code: {(int) c}");
            path = $"{WebServer._basePath}/{path}";
            // Console.WriteLine($"Trying to read {path} !");
            string file = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(file);
        }

        internal static bool WriteObjecToJSON<T>(string Path, T user)
        {
            // First, convert the object to JSON.
            string JSONData = JsonSerializer.Serialize(user);
            try
            {
                File.WriteAllText(Path, JSONData);
                return true; 
            } catch
            {
                return false;
            }
        }
    }
}