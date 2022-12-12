namespace WebServer
{
    using System.Text.Json;
    using System.IO;

    public class Helpers
    {
        public static string ObjectToJSON(object o)
        {
            return JsonSerializer.Serialize(o);
        }

        private static Dictionary<string, string> _mimeTypes = new Dictionary<string, string>()
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
    }
}