using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace Alice.Discord {
	public static class TStorageStatic {
		private static long lastEdit = DateTimeOffset.Now.ToUnixTimeSeconds();
		public static async Task Download(SocketUserMessage message) {
			try {
				RestUserMessage msg;
				if (IsValidUri(message.Content)) {
					msg = await message.Channel.SendMessageAsync("Found TStorage link");
				}
				else return;
				
				var fileName = CreateMd5(message.Content);
				if (File.Exists(path: $"backup/{fileName}")) {
					await msg.ModifyAsync(e => e.Content = $"File [{fileName}] already exists.");
					return;
				}
				
				HttpClientExtension client = new HttpClientExtension();
				client.Progress += delegate(object sender, ProgressArgs args)
				{
					if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastEdit < 5) {
						return;
					}

					lastEdit = DateTimeOffset.Now.ToUnixTimeSeconds();
					msg.ModifyAsync(e =>
						e.Content =
							$"[{fileName}]  {args.Progress:F2}% downloaded  ({args.LoadedBytes}/{args.TotalBytes})");
				};
				FileStream fs = new FileStream($"backup/{fileName}", FileMode.CreateNew, FileAccess.Write);
				client.Data += async delegate(object sender, byte[] args)
				{
					await fs.WriteAsync(args, (int)fs.Length, args.Length);
				};
				client.Finish += async delegate
				{
					await msg.ModifyAsync(e => e.Content = $"File [{fileName}] was saved.");
					await fs.DisposeAsync();
				};
				
				await client.AsyncSend(new HttpClient(), new HttpRequestMessage(HttpMethod.Get, message.Content) {
					Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
						new KeyValuePair<string, string>("op", 				"download2"),
						new KeyValuePair<string, string>("id", 				message.Content.Substring(message.Content.LastIndexOf('/') + 1)),
						new KeyValuePair<string, string>("rand", 			string.Empty),
						new KeyValuePair<string, string>("referer", 		string.Empty),
						new KeyValuePair<string, string>("method_free", 	string.Empty),
						new KeyValuePair<string, string>("method_premium", 	string.Empty)
					})
				});
			}
			catch (Exception e) {
				Console.WriteLine(e);
				await message.Channel.SendMessageAsync("AN ERROR HAPPEND, PLEASE IMMEDIATELY MESSAGE SPACEFOX");
			}
		}
		
		private static bool IsValidUri(string link) {
			return new Regex("^http://tstorage\\.info/[a-zA-Z0-9]+/?$").IsMatch(link);
		}

		private static string CreateMd5(string input) {
			using var md5 = System.Security.Cryptography.MD5.Create();
			var calculatedHash = md5.ComputeHash(Encoding.ASCII.GetBytes(input))
				.Aggregate(string.Empty,
					(current, item) => current + item.ToString("X2"));
			return calculatedHash;
		}
	}
}