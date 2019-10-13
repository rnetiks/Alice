using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace Alice.Discord.Modules
{
    public class BotUtility : ModuleBase<SocketCommandContext>
    {
        HttpClient GetHttpClient = new HttpClient();
        const int HentaiFoxMaxID = 60000;
        
        public class HentaiContainer
        {
            public int ImageCount;
            public string Name;
            public ulong Id; //Int64 ID of the Message
            public int CurrentPage = 0; //0: Cover, 1-2147483647: Pages
            public string Path; //Absolute Path to the Gallery
            public ulong RequestAuthor; //Int64 ID of the User which has Requested the Gallery
        }
        public static List<HentaiContainer> interactiveMessages = new List<HentaiContainer>();
        WebClient hentaiClient = new WebClient();

        
        [Command("hentaifox")]
        private async Task CommandHentai(int id)
        {
            const string hentaifoxUrl = "https://hentaifox.com/gallery/{0}";
            const string coverRegex = @"cover.*?src=""(.*?)"".*?info.*?h1>(.*?)</h1.*?Pages: (\d+)";

            var webpage = hentaiClient.DownloadString(string.Format(hentaifoxUrl, id));

            var regex = new Regex(coverRegex, RegexOptions.Singleline);
            var match = regex.Match(webpage);


            var embed = new EmbedBuilder
            {
                ImageUrl = "https:" + match.Groups[1],
                Title = match.Groups[2].ToString().Replace("&#39;", "'"),
                Color = Color.Blue
            };

            int.TryParse(match.Groups[3].Value, out var pages);
            embed.AddField("Pages", pages);

            var msg = await Context.Channel.SendMessageAsync(string.Empty, false, embed.Build());

            var right = new Emoji("▶");
            await msg.AddReactionAsync(right);
            var hhh = match.Groups[1].ToString();
            interactiveMessages.Add(new HentaiContainer()
            {
                Id = msg.Id,
                CurrentPage = 0,
                Name = match.Groups[2].ToString(),
                ImageCount = pages,
                Path = "https:" + match.Groups[1].ToString().Substring(0, hhh.LastIndexOf('/')),
                RequestAuthor = Context.User.Id
            });
        }

        [Command("hentairdm")]
        public async Task RandomHentai()
        {
            await CommandHentai(Program.Random.Next(HentaiFoxMaxID));
        }


        [Command("dbtag"), RequireNsfw()]
        public async Task GetTag(string searchString)
        {
            var webClient = new WebClient();
            var json = webClient.DownloadString($"https://danbooru.donmai.us/tags.json?search[name_matches]=*{searchString}*&search[order]=count");
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<TagSearch[]>(json);
            var h = string.Empty;
            for (var i = 0; i < t.Length; i++)
            {
                //Escapes Discord Italic Formatting
                h = h += t[i].Name.Replace("_", "\\_") + $" ({t[i].PostCount})" + "\n";
                if (i > 5) continue;
            }

            var b = new EmbedBuilder {Description = h};
            await Context.Channel.SendMessageAsync(string.Empty, false, b.Build());
        }

        public class TagSearch
        {
            public string Name;
            public string PostCount;
        }
        
        [Command("pat")]
        public async Task Pat(IUser s)
        {
            await Context.Channel.SendMessageAsync($"Patted {s.Mention}");
        }

        /// <summary>
        /// Translates a text to a given Language
        /// </summary>
        /// <param name="lang">inputs like [en-jp] to translate from english to japanese</param>
        /// <param name="text">the text that should get translated</param>
        /// <returns></returns>
        //[Command("translate")] /*Remove '//' once the method is finished*/
        public async Task TranslateTo(string lang, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                var id = await Context.Channel.SendMessageAsync("No text to convert");
                var t = new Timer {AutoReset = false, Interval = 10000};
                t.Start();
                t.Elapsed += async delegate
                {
                    t.Stop();
                    t.Dispose();
                    await id.DeleteAsync();
                };
            }
            if (string.IsNullOrEmpty(lang))
            {
                var id = await Context.Channel.SendMessageAsync("Missing Argument: [Language]");
                var t = new Timer(10000) {AutoReset = false};
                t.Start();
                t.Elapsed += async delegate
                {
                    t.Stop();
                    t.Dispose();
                    await id.DeleteAsync();
                };
            }
            const string googleApi = "";
            var w = await GetHttpClient.GetAsync(googleApi);
            var s = await w.Content.ReadAsStringAsync();

            var convertContent = new Regex("", RegexOptions.Singleline);
            await Context.Channel.SendMessageAsync(s);
        }

        WebClient wc = new WebClient();
        static List<string> bannedTags = new List<string>();
        
        [Command("db")]
        public async Task GetImage(string tag)
        {
            if (bannedTags.Contains(tag.ToLowerInvariant()))
            {
                await Context.Channel.SendMessageAsync($"Tag [{tag}] is banned from usage");
                return;
            }
            
            var jsonString = wc.DownloadString($"https://danbooru.donmai.us/posts.json?tags={tag}&limit=1&random=true");

            var image = JsonConvert.DeserializeObject<List<DbImageGetClass>>(jsonString)[0];

            var eb = new EmbedBuilder {Color = Color.Blue, ImageUrl = image.Url};
            eb.AddField("Artist:", image.Artist.Replace("_", "\\_"));
            eb.AddField("Tags:", image.Tags.Replace("_", "\\_"));
            
            await Context.Channel.SendMessageAsync(string.Empty, false, eb.Build());
        }
        
        public class DbImageGetClass
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
