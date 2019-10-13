using System;
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

            string game = args[1];
            if (args[1] == string.Empty){
                 game = "Development Build";
            }
            var core = new Core();
            await core.RunBotAsync(args[0], game);
            await Task.Delay(-1);
        }
    }
}
