using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Alice.Discord
{
    internal static class Program
    {
        public static readonly Random Random = new Random();
        
        public static async Task Main(string[] args)
        {
            if (!File.Exists("apiKEY"))
            {
                File.WriteAllText("apiKEY", "");
            }
            var apiKEY = File.ReadAllText("apiKEY");
            if (apiKEY.Length <= 16)
            {
                Console.WriteLine("no API Key Found, please add one in the \"apiKEY\" File");
                Console.ReadKey(true);
                return;
            }
            var core = new Core();
            await core.RunBotAsync(apiKEY, "DevBuild");
            await Task.Delay(-1);
        }
    }
}
