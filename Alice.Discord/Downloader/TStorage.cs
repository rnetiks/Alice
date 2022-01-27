using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using static Alice.Discord.HttpClientExtension;

namespace Alice.Discord {
	
	public static class TStorage {
		private static long lastEdit = DateTimeOffset.Now.ToUnixTimeSeconds();
		public static async Task CheckTStorage(SocketUserMessage message) {
			try {
				RestUserMessage msg = null;
				if (IsValid(message.Content)) {
					msg = await message.Channel.SendMessageAsync("Found TStorage link");
				}
				else return;
				var fileName = CreateMD5(message.Content);
				if (File.Exists(path: $"backup/{fileName}")) {
					await msg.ModifyAsync(e => e.Content = $"File [{fileName}] already exists.");
					return;
				}
				
				HttpClient client = new HttpClient();
				await client.AsyncSend(new HttpRequestMessage(HttpMethod.Post, message.Content) {
					Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>> {
						new KeyValuePair<string, string>("op", 				"download2"),
						new KeyValuePair<string, string>("id", 				message.Content.Substring(message.Content.LastIndexOf('/') + 1)),
						new KeyValuePair<string, string>("rand", 			string.Empty),
						new KeyValuePair<string, string>("referer", 		string.Empty),
						new KeyValuePair<string, string>("method_free", 	string.Empty),
						new KeyValuePair<string, string>("method_premium", 	string.Empty)
					})
				}, args =>
				{
					if (DateTimeOffset.Now.ToUnixTimeSeconds() - lastEdit < 3) return;
					lastEdit = DateTimeOffset.Now.ToUnixTimeSeconds();
					msg.ModifyAsync(e => e.Content = $"[{fileName}] {args.Progress:F2}% downloaded # {args.TotalBytes}");
				}, bytes =>
				{
					File.WriteAllBytes($"backup/{fileName}", bytes);
					msg.ModifyAsync(e => e.Content = $"File [{fileName}] was saved. {bytes.Length}");
				});
			}
			catch (Exception e) {
				Console.WriteLine(e);
				await message.Channel.SendMessageAsync("AN ERROR HAPPEND, PLEASE IMMEDIATELY MESSAGE SPACEFOX");
			}
		}

		private static bool IsValid(string link) {
			var regex = new Regex("^http://tstorage\\.info/[a-zA-Z0-9]+$");
			return regex.IsMatch(link);
		}
		
		public static string CreateMD5(string input)
		{
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] inputBytes = Encoding.ASCII.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);
				StringBuilder sb = new StringBuilder();
				foreach (var t in hashBytes) {
					sb.Append(t.ToString("X2"));
				}
				return sb.ToString();
			}
		}
	}
}