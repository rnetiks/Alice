using System;
using System.Threading.Tasks;

namespace Alice.Discord
{
    internal static class Program
    {
        public static readonly Random Random = new Random();
        
        public static async Task Main(string[] args) {
            Console.Write("Enter Id: ");
            var id = "MjQ2ODIxMTg4ODIxNzEyODk3.WCZ8JQ.W7TtOAzvd9cCAKttJfQzbUkCy3U";//Console.ReadLine();
            var game = "Development Build";
            var core = new Core();
            await core.RunBotAsync(id, game);
            await Task.Delay(-1);
        }
    }
}