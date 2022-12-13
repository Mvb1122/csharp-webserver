using System.Net;
using System.Text;
using ResponseInformation = WebServer.ResponseInformation;


namespace Main
{
    internal class Methods
    {
        public static readonly Func<HttpListenerRequest, ResponseInformation>[] Functions = { ExampleRequest, v1_FolderedMethod, RandomNumber, STDev };

        public static ResponseInformation ExampleRequest(HttpListenerRequest request)
        {
            ResponseInformation response = new(request, WebServer.Helpers.GetMime(".txt"), $"Reached! Your URL is: {request.Url.LocalPath}");
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
            return new ResponseInformation(request, WebServer.Helpers.GetMime(".json"), Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(r)));
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

            if (req.QueryString["numbers"] != null)
            {
                // Get the list of numbers.
                List<float> numbersList = new List<float>(0);
                string[] numbersNotParsed = req.QueryString.Get("numbers").Split(",");
                foreach (string number in numbersNotParsed) numbersList.Add(float.Parse(number));
                numbersList.Sort();
                float[] numbers = numbersList.ToArray();

                // Find average.
                float avg = numbers.Average();

                // Find the STDev.
                float STSum = (float) numbers.Sum(x => Math.Pow(x - avg, 2));

                response.STDev = (float) Math.Sqrt(STSum / numbers.Length);
                response.Sucessful = true;
            }

            return new ResponseInformation(req, response);
        }

        class STDevResponse
        {
            public bool Sucessful { get; set; }
            public double STDev { get; set; }
        }
    }
}