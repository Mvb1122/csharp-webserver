namespace Main
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            var ws = new WebServer.WebServer("http://*:6400/"); // http://73.127.135.113:6400/api/game/GetServersList/

            WebServer.WebServer.AddMethod(Methods.Functions);
            WebServer.WebServer.AddMethod(ServerMethods.Functions);

            ws.Run();
            int NumTimesToHitStop = 2;
            Console.WriteLine($"Press a key {NumTimesToHitStop} time{(NumTimesToHitStop > 1 ? "s" : "")} to exit.");
            for (int i = 0; i < NumTimesToHitStop; i++)
                Console.ReadKey();


            ws.Stop(); 
        }
    }
}

/*
 *  makecert -n "CN=MicahBDev" -r -sv MicahBDev.pvk MicahBDev.cer
 *  Install Cert on computer via file explorer, move to admin dev command prompt.
 *  makecert -sk MicahBDevByCA -iv MicahBDev.pvk -n "CN=MicahBDev" -ic MicahBDev.cer MicahBDevSignedByCA.cer -sr localmachine -ss My
 *  netsh http add sslcert ipport=0.0.0.0:6400 certhash=0be843ef3231e24eae609a041fca56511586f4a4 appid=`{E442FA81-E0C3-4B8C-8F7C-E4EE32E23CAD`}
 */