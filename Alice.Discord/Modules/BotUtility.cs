using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Alice.Discord.Modules
{
    public class BotUtility : ModuleBase<SocketCommandContext>
    {
        public class HentaiContainer
        {
            public int ImageCount;
            public string Name;
            public ulong id;
            public int currentPage = 0;
            public string path;
            public ulong RequestAuthor;
        }
        
        public static List<HentaiContainer> interactiveMessages = new List<HentaiContainer>();
        WebClient hentaiClient = new WebClient();
        
        [Command("hentai")]
        private async Task CommandHentai(int id)
        {
            const string hentaifoxUrl = "https://hentaifox.com/gallery/{0}";
            const string coverRegex = @"cover.*?src=""(.*?)"".*?info.*?h1>(.*?)</h1.*?Pages: (\d+)";
            
            string webpage = hentaiClient.DownloadString(string.Format(hentaifoxUrl, id));
            
            var regex = new Regex(coverRegex, RegexOptions.Singleline);
            var match = regex.Match(webpage);
            
            
            var embed = new EmbedBuilder();
            embed.ImageUrl = "https:" + match.Groups[1];
            embed.Title = match.Groups[2].ToString().Replace("&#39;", "'");
            embed.Color = Color.Blue;

            int.TryParse(match.Groups[3].Value, out int pages);
            embed.AddField("Pages", pages);
            
            var msg = await Context.Channel.SendMessageAsync(string.Empty, false, embed.Build());
            
            var right = new Emoji("▶");
            await msg.AddReactionAsync(right);
            string hhh = match.Groups[1].ToString();
            interactiveMessages.Add(new HentaiContainer()
            {
                id = msg.Id,
                currentPage = 0,
                Name = match.Groups[2].ToString(),
                ImageCount = pages,
                path = "https:"+match.Groups[1].ToString().Substring(0, hhh.LastIndexOf('/')),
                RequestAuthor = Context.User.Id
            });
            for (int i = 0; i < interactiveMessages.Count; i++)
            {
                Console.WriteLine(interactiveMessages[i]);
            }
        }
        
        [Command("hentairdm")]
        public async Task d()
        {
            Random rdm = new Random();
            await CommandHentai(rdm.Next(50000));
        }
        
        
        [Command("dbtag")]
        public async Task getTag(string searchString)
        {
            WebClient wc = new WebClient();
            string json = wc.DownloadString($"https://danbooru.donmai.us/tags.json?search[name_matches]=*{searchString}*&search[order]=count");
            tagSearch[] t = Newtonsoft.Json.JsonConvert.DeserializeObject<tagSearch[]>(json);
            string h = string.Empty;
            for (int i = 0; i < t.Length; i++)
            {
                h = h += t[i].name.Replace("_","\\_")+$" ({t[i].post_count})"+"\n";
                if (i > 5) continue;
            }
            EmbedBuilder b = new EmbedBuilder();
            b.WithDescription(h);
            Console.WriteLine(t[0].name);
            await Context.Channel.SendMessageAsync(string.Empty, false, b.Build());
        }
        
        public class tagSearch
        {
            public string name;
            public string post_count;
        }
        
        [Command("tagunban")]
        public async Task unbanTag(string tag)
        {
            if (Context.User.Id == 168407391317000192)
            {
                bannedTags.Remove(tag.ToLowerInvariant());
                await Context.Channel.SendMessageAsync($"Unbanned Tag: [{tag}]");

            }
            else
            {
                await Context.Channel.SendMessageAsync("You are not an Administrator");
            }
        }
        
        [Command("8ball")]
        public async Task ball8([Remainder] string s)
        {
            string[] h = { "Yes", "No", "Maybe", "I dont know", "Possibly", "I highly doubt it", "Definetely", "Most likely" };
            await Context.Channel.SendMessageAsync(h[Program.Random.Next(h.Length)]);
        }
        
		[Command("pat")]
		public async Task pat(string s)
        {
			await Context.Channel.SendMessageAsync($"Patted {s}");
		}
        
        [Command("tagban")]
        public async Task banTag(string tag)
        {
            if (Context.User.Id == 168407391317000192)
            {
                bannedTags.Add(tag.ToLowerInvariant());
                await Context.Channel.SendMessageAsync($"Banned Tag: [{tag}]");
            }
            else 
            {
                await Context.Channel.SendMessageAsync("You are not an Administrator");
            }
        }
        
        WebClient wc = new WebClient();
        static List<string> bannedTags = new List<string>();
        [Command("db")]
        public async Task getImage(string tag)
        {
            if (bannedTags.Contains(tag.ToLowerInvariant()))
            {
                await Context.Channel.SendMessageAsync($"Tag [{tag}] is banned from usage");
                return;
            }
            
            string jsonString = wc.DownloadString($"https://danbooru.donmai.us/posts.json?tags={tag}&limit=1&random=true");

            var image = JsonConvert.DeserializeObject<List<DBImageGetClass>>(jsonString)[0];

            var eb = new EmbedBuilder();
            eb.Color = Color.Blue;
            eb.ImageUrl = image.Url;
            eb.AddField("Artist:", image.Artist.Replace("_", "\\_"));
            eb.AddField("Tags:", image.Tags.Replace("_", "\\_"));
            
            await Context.Channel.SendMessageAsync(string.Empty, false, eb.Build());
        }
        
        public class DBImageGetClass
        {
            [JsonProperty("file_url")]
            public string Url;
            
            [JsonProperty("tag_string")]
            public string Tags;
            
            [JsonProperty("tag_string_artist")]
            public string Artist;
        }

    }
}
