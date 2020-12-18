using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace Alice.Discord.Modules
{
    public class BotUtility : ModuleBase<SocketCommandContext>
    {
        HttpClient GetHttpClient = new HttpClient();
        WebClient WebClient = new WebClient();
        
        #region Hentaifox

        const int HentaiFoxMaxID = 60000;
        
        public class HentaiContainer
        {
            public int ImageCount;
            public string Name;
            public ulong Id; //Int64 ID of the Message
            public int CurrentPage = 0; //0: Cover, 1-2147483647: Pages
            public string Path; //Absolute Path to the Gallery
            public ulong RequestAuthor; //Int64 ID of the User which has Requested the Gallery
            public RestUserMessage MessageLink { get; set; }
        }
        public static List<HentaiContainer> InteractiveMessages = new List<HentaiContainer>();

        
        [Command("hf")]
        private async Task commandHF(int id)
        {
            const string hentaifoxUrl = "https://hentaifox.com/gallery/{0}/";
            HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
            
            const string coverRegex = @"cover.*?src=""(.*?)"".*?info.*?h1>(.*?)</h1.*?Pages: (\d+)";

            var webpage = WebClient.DownloadString(string.Format(hentaifoxUrl, id));
            doc.LoadHtml(webpage);
            var thumbnail = doc.DocumentNode.QuerySelector(".cover").QuerySelector("img").GetAttributeValue("data-cfsrc", "");
            var title = doc.DocumentNode.QuerySelector(".info").QuerySelector("h1").InnerText;
            var regex = new Regex(coverRegex, RegexOptions.Singleline);
            var match = regex.Match(webpage);
            var embed = new EmbedBuilder
            {
                ImageUrl = thumbnail,
                Title = title,
                Color = Color.Blue
            };

            int.TryParse(match.Groups[3].Value, out var pages);
            embed.AddField("Pages", pages);

            var msg = await Context.Channel.SendMessageAsync(string.Empty, false, embed.Build());
            var right = new Emoji("▶");
            var cross = new Emoji("✖");
            var check = new Emoji("✔");
            IEmote[] emotes = {right};
            await msg.AddReactionsAsync(emotes);
            var hhh = match.Groups[1].ToString();
            InteractiveMessages.Add(new HentaiContainer()
            {
                Id = msg.Id,
                CurrentPage = 0,
                Name = match.Groups[2].ToString(),
                ImageCount = pages,
                Path = thumbnail.Substring(0, thumbnail.LastIndexOf('/')),
                RequestAuthor = Context.User.Id,
                MessageLink = msg
            });
        }

        [Command("hfrdm")]
        public async Task RandomHentai()
        {
            await commandHF(Program.Random.Next(HentaiFoxMaxID));
        }

        #endregion

        [Command("test")]
        public async Task getTestResul()
        {
            await Context.Channel.SendMessageAsync("Online.");
        }

        #region Help

        [Command("help")]
        public async Task showHelp(string s = "") {
            if (s == "") {
                await Context.Channel.SendMessageAsync("To be implemented");
            }
            else {
                switch (s) {
                    case "nl":
                        await Context.Channel.SendMessageAsync(
                            "a!nl <'femdom', 'tickle', 'classic', 'ngif', 'erofeet', 'meow', 'erok', 'poke', 'les', 'v3', 'hololewd', 'nekoapi_v3.1', 'lewdk', 'keta', 'feetg', 'nsfw_neko_gif', 'eroyuri', 'kiss', '8ball', 'kuni', 'tits', 'pussy_jpg', 'cum_jpg', 'pussy', 'lewdkemo', 'lizard', 'slap', 'lewd', 'cum', 'cuddle', 'spank', 'smallboobs', 'goose', 'Random_hentai_gif', 'avatar', 'fox_girl', 'nsfw_avatar', 'hug', 'gecg', 'boobs', 'pat', 'feet', 'smug', 'kemonomimi', 'solog', 'holo', 'wallpaper', 'bj', 'woof', 'yuri', 'trap', 'anal', 'baka', 'blowjob', 'holoero', 'feed', 'neko', 'gasm', 'hentai', 'futanari', 'ero', 'solo', 'waifu', 'pwankg', 'eron', 'erokemo'>");
                        break;
                }
            }
        }

        #endregion
        #region Nekos.Life
            public class NekosLife
            {
                [JsonProperty("url")]
                public string Image;
            }
            [Command("nl")]
            public async Task GetNLApi(string s)
            {
                string nekoApi = $"https://nekos.life/api/v2/img/{s}";
                await SendNekoApi(nekoApi);
            }

            public async Task SendNekoApi(string nekolifeApi) {
                var s = WebClient.DownloadString(nekolifeApi);
                var e = Newtonsoft.Json.JsonConvert.DeserializeObject<NekosLife>(s);
                var embedBuilder = new EmbedBuilder {Color = Color.Blue, ImageUrl = e.Image};
                await Context.Channel.SendMessageAsync(null, false, embedBuilder.Build());
            }
        #endregion
        
        #region Danbooru

        [Command("dbtag")]
        public async Task GetTag(string searchString)
        {
            
            var json = WebClient.DownloadString($"https://danbooru.donmai.us/tags.json?search[name_matches]=*{searchString}*&search[order]=count");
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
            [JsonProperty("name")]
            public string Name;
            [JsonProperty("post_count")]
            public string PostCount;
        }

        static List<string> bannedTags = new List<string>();
        
        [Command("db")]
        public async Task GetImage([Remainder] string tag)
        {
            if (bannedTags.Contains(tag.ToLowerInvariant()))
            {
                await Context.Channel.SendMessageAsync($"Tag [{tag}] is banned from usage");
                return;
            }

            if (tag.Trim().Replace("rating:safe", "").Replace("rating:questionable", "").Replace("rating:explicit", "")
                    .Split(' ').Length > 2) {
                await Context.Channel.SendMessageAsync("Cant use more than 2 tags");
                return;
            }
            
            var jsonString = WebClient.DownloadString($"https://danbooru.donmai.us/posts.json?tags={tag}&limit=1&random=true");

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
        #endregion
        
        [Command("pat")]
        public async Task Pat(IUser s)
        {
            await Context.Channel.SendMessageAsync($"Patted {s.Mention}");
        }
    }
}
