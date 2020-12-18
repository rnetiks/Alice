using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Alice.Discord
{
    internal static class Program
    {
        public static readonly Random Random = new Random();
        
        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Please supply your authentication token as argument.");
                return;
            }

            var game = "Development Build";
            if (args.Length >= 2 && game != string.Empty)
                game = args[1];
            
            var core = new Core();
            await core.RunBotAsync(args[0], game);
            await Task.Delay(-1);
        }
    }
}