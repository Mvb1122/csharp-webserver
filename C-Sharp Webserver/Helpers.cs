using System;

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

        public static string DictionaryToJSON(Dictionary<string, string> JSONData)
        {
            string data = "{";
            foreach (string key in JSONData.Keys)
                data += $"\n\t\"{key}\": \"{JSONData[key]}\",";
            return string.Concat(data.AsSpan(0, data.Length - 1), "\n}");
        }
        public static float SumArray(float[] input)
        {
            float sum = 0f;
            foreach (float value in input) sum += value;
            return sum;
        }

        public static bool ArrayContains<T>(T[] array, T val, out int index)
        {
            for (int i = 0; i < array.Length; i++) if (array[i].Equals(val))
                {
                    index = i;
                    return true;
                }

            index = -1;
            return false;
        }

        public static int[] GetCharacterIndexesInString(char character, string stringToSearch)
        {
            List<int> indexes = new List<int>(0);
            for (int i = 0; i < stringToSearch.Length; i++) if (stringToSearch.ToCharArray()[i] == character) indexes.Add(i);
            return indexes.ToArray();
        }

        public static string ReverseString(string st)
        {
            // Change the string into an array, flip the array, and then turn it back into a string and send it back.
            char[] s = st.ToCharArray();
            Array.Reverse(s);
            return new string(s);
        }
    }

}