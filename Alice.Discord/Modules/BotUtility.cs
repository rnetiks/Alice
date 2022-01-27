using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SharpCompress.Archives;

// ReSharper disable UnusedMember.Global

namespace Alice.Discord.Modules
{
    /// <inheritdoc />
    [UsedImplicitly]
    public partial class BotUtility : ModuleBase<SocketCommandContext>
    {
        private readonly WebClient _webClient = new WebClient();
        private readonly Random _random = new Random();

        #region Hentaifox

        private const int HentaiFoxMaxId = 81739;
        public static List<HentaiContainer> InteractiveMessages = new List<HentaiContainer>();
        
        
        [Command("hentaifox"), RequireNsfw]
        public async Task commandHF(int id)
        {
            const string hentaifoxUrl = "https://hentaifox.com/gallery/{0}/";
            HtmlDocument htmlDocument = new HtmlDocument();
            
            const string coverRegex = @"cover.*?src=""(.*?)"".*?info.*?h1>(.*?)</h1.*?Pages: (\d+)";

            var webpage = _webClient.DownloadString(string.Format(hentaifoxUrl, id));
            htmlDocument.LoadHtml(webpage);
            var thumbnail = htmlDocument.DocumentNode.QuerySelector(".cover").QuerySelector("img").GetAttributeValue("data-cfsrc", "");
            var title = htmlDocument.DocumentNode.QuerySelector(".info").QuerySelector("h1").InnerText;
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

            var message = await Context.Channel.SendMessageAsync(string.Empty, false, embed.Build());
            var right = new Emoji("▶");
            var cross = new Emoji("✖");
            var check = new Emoji("✔");
            IEmote[] emotes = {right};
            await message.AddReactionsAsync(emotes);
            InteractiveMessages.Add(new HentaiContainer()
            {
                Id = message.Id,
                CurrentPage = 0,
                Name = match.Groups[2].ToString(),
                ImageCount = pages,
                Path = thumbnail.Substring(0, thumbnail.LastIndexOf('/')),
                RequestAuthor = Context.User.Id,
                MessageLink = message
            });
        }

        [Command("hentaifoxr"), RequireNsfw]
        public async Task RandomHentai()
        {
            await commandHF(_random.Next(HentaiFoxMaxId));
        }

        #endregion

        #region Help

        #endregion
        #region Nekos.Life
            public class NekosLife
            {
                [JsonProperty("url")]
                public string Image;
            }
            [Command("nekoslife"), RequireNsfw]
            public async Task GetNLApi(string input)
            {
                var nekoApi = $"https://nekos.life/api/v2/img/{input}";
                await SendNekoApi(nekoApi);
            }

            public async Task SendNekoApi(string nekolifeApi) {
                var value = _webClient.DownloadString(nekolifeApi);
                var deserializeObject = JsonConvert.DeserializeObject<NekosLife>(value);
                var embedBuilder = new EmbedBuilder {
                    Color = Color.Blue, 
                    ImageUrl = deserializeObject.Image
                };
                await Context.Channel.SendMessageAsync(null, false, 
                    embedBuilder.Build());
            }
        #endregion
        
        #region Danbooru

        [Command("danboorutags"), RequireNsfw]
        public async Task GetTag(string searchString)
        {
            var json = _webClient.DownloadString($"https://danbooru.donmai.us/tags.json?search[name_matches]={searchString}&search[order]=count");
            var tagSearches = JsonConvert.DeserializeObject<TagSearch[]>(json);
            var description = string.Empty;
            for (var i = 0; i < tagSearches.Length; i++)
            {
                //Escapes Discord Italic Formatting
                description += tagSearches[i].Name.Replace("_", "\\_") + $" ({tagSearches[i].PostCount})" + "\n";
                if (i > 5) continue;
            }

            var b = new EmbedBuilder {Description = description};
            await Context.Channel.SendMessageAsync(string.Empty, false, b.Build());
        }

        public class TagSearch
        {
            [JsonProperty("name")]
            public string Name;
            [JsonProperty("post_count")]
            public string PostCount;
        }
        
        
        [Command("danbooru", RunMode = RunMode.Async), RequireNsfw]
        public async Task GetImage([Remainder] string tag = "") {
            int count = 1;
            var arguments = tag.Split();
            foreach (var argument in arguments) {
                if (!argument.StartsWith("--count=")) continue;
                var substring = argument.Substring(8);
                if (!int.TryParse(substring, out int b)) continue;
                if (b > 0 && b <= 5) count = b;
            }

            try {
                var jsonString = _webClient.DownloadString($"https://danbooru.donmai.us/posts.json?tags={tag}&limit={count}&random=true");
                var image = JsonConvert.DeserializeObject<List<DbImageGetClass>>(jsonString);
                string seed = string.Empty;
                if (image != null && image.Count > 0) {
                    seed = image.Where(t => !string.IsNullOrEmpty(t.Url)).
                        Aggregate(seed, (current, t) => current + (t.Url + "\n"));

                    await Context.Channel.SendMessageAsync(seed);
                }
                else {
                    if (image == null) {
                        await Context.Channel.SendMessageAsync("API response returned error");
                        return;
                    }

                    if (image.Count < 1) {
                        await Context.Channel.SendMessageAsync("API response was empty");
                    }
                }
            }
            catch (Exception e) {
                await Context.Channel.SendMessageAsync("Unknown error");
            }
        }

        #endregion
        
        [Command("pat")]
        public async Task Pat(IUser s)
        {
            await Context.Channel.SendMessageAsync($"Patted {s.Mention}");
        }
        
    }

}
