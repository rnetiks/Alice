using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace Alice.Discord
{
    internal static class Program
    {
        private static string MakeValidFileName( string name ) {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape( new string( System.IO.Path.GetInvalidFileNameChars() ) );
            string invalidRegStr = string.Format( @"([{0}]*\.+$)|([{0}]+)", invalidChars );

            return System.Text.RegularExpressions.Regex.Replace( name, invalidRegStr, "_" );
        }
        public static async Task Main(string[] args) {

            try {
                if (args.Length < 1) {
                    Console.WriteLine("Please supply your authentication token as argument.");
                    Console.ReadKey();
                    return;
                }
                
                var game = "Your local foxgirl loli";
                if (args.Length >= 2 && !string.IsNullOrWhiteSpace(args[1])) game = args[1];
                var applicationCore = new ApplicationCore();
                await applicationCore.RunBotAsync(args[0], game);

            }
            catch (Exception e) {
                Console.WriteLine(e);
                Console.ReadKey();
                return;
            }
            await Task.Delay(-1);

        }
    }
}