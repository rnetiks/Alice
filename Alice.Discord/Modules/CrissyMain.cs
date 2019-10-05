using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using AliceIMGLibrary = NReco.ImageGenerator;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Discord;
using System.Text.RegularExpressions;

namespace DiscordBot.Modules
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
        public static List<HentaiContainer> l = new List<HentaiContainer>();
        WebClient hentaiClient = new WebClient();
        [Command("hentai")]
        public async Task getHentaiGalleryByID(int id)
        {
            string s = hentaiClient.DownloadString("https://hentaifox.com/gallery/" + id);
            s = s.Replace("\n", "");
            Console.WriteLine(s.Length);
            Regex regex = new Regex("cover.*?src=\"(.*?)\".*?info.*?h1>(.*?)</h1.*?class=\"gallery\">(.*?)<div class=\"clear\"");
            Match m = regex.Match(s);
            EmbedBuilder embedBuilder = new EmbedBuilder();
            Console.WriteLine(m.Groups[1].ToString());
            embedBuilder.WithImageUrl("https:" + m.Groups[1].ToString());
            embedBuilder.WithTitle(m.Groups[2].ToString().Replace("&#39;", "'"));
            embedBuilder.WithColor(Discord.Color.Blue);
            string lengthImageString = m.Groups[3].Value;
            regex = new Regex("<div.*?>(.*?)</div>");
            MatchCollection v = regex.Matches(lengthImageString);
			int imagesCount = v.Count;
            embedBuilder.AddField("Pages", imagesCount.ToString());
            var msg = await Context.Channel.SendMessageAsync(string.Empty, false, embedBuilder.Build());
            //var msg = await Context.Channel.SendMessageAsync(m.Groups[1].ToString());
            var right = new Emoji("▶");
            await msg.AddReactionAsync(right);
            string hhh = m.Groups[1].ToString();
            l.Add(new HentaiContainer()
            {
                id = msg.Id,
                currentPage = 0,
                Name = m.Groups[2].ToString(),
                ImageCount = imagesCount,
                path = "https:"+m.Groups[1].ToString().Substring(0, hhh.LastIndexOf('/')),
                RequestAuthor = Context.User.Id
            });
            for (int i = 0; i < l.Count; i++)
            {
                Console.WriteLine(l[i]);
            }
        }
        [Command("hentairdm")]
        public async Task d()
        {
            Random rdm = new Random();
            await getHentaiGalleryByID(rdm.Next(50000));
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
                if (i > 5)
                {
                    //this is a comment, it shouldnt do anything but for some reason without it the
                    //program wont run
                    continue;
                }
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
                bannedTags.Remove(tag);
                await Context.Channel.SendMessageAsync($"Unbanned Tag: [{tag}]");

            }
            else
            {
                await Context.Channel.SendMessageAsync("You are not an Administrator");
            }
        }
        Random rmd = new Random();
        [Command("8ball")]
        public async Task ball8([Remainder] string s)
        {
            string[] h = new string[] { "Yes", "No", "Maybe", "I dont know", "Possibly", "I highly doubt it", "Definetely", "Most likely" };
            await Context.Channel.SendMessageAsync(h[rmd.Next(h.Length)]);
        }
		[Command("pat")]
		public async Task pat(string s){
			await Context.Channel.SendMessageAsync($"Patted {s}");
		}
        [Command("tagban")]
        public async Task banTag(string tag)
        {
            if (Context.User.Id == 168407391317000192)
            {
                bannedTags.Add(tag);
                await Context.Channel.SendMessageAsync($"Banned Tag: [{tag}]");
            }
            else {
                await Context.Channel.SendMessageAsync("You are not an Administrator");
            }
        }
        WebClient wc = new WebClient();
        static List<string> bannedTags = new List<string>();
        [Command("db")]
        public async Task getImage(string tag)
        {
            if (bannedTags.ToArray().Length > 0)
            {
                Console.WriteLine(bannedTags[0]);
                for (int x = 0; x < bannedTags.ToArray().Length; x++)
                {
                    if (bannedTags[x].Contains(tag))
                    {
                        await Context.Channel.SendMessageAsync($"Tag [{bannedTags[x]}] is banned from usage");
                        return;
                    }
                }

            }
            Console.WriteLine("WebClient Created");
            string jsonString = wc.DownloadString($"https://danbooru.donmai.us/posts.json?tags={tag}&limit=1&random=true");
            Console.WriteLine("Json String got");

            db_image_get_class[] db_Image = Newtonsoft.Json.JsonConvert.DeserializeObject<db_image_get_class[]>(jsonString);

            try
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.AddField("Artist:", db_Image[0].tag_string_artist.Replace("_","\\_"));
                eb.AddField("Tags:", db_Image[0].tag_string.Replace("_", "\\_"));
                eb.WithColor(Discord.Color.Blue);
                eb.WithImageUrl(db_Image[0].file_url);
                await Context.Channel.SendMessageAsync(string.Empty, false, eb.Build());
            }
            catch (Exception e)
            {
                EmbedBuilder eb = new EmbedBuilder();
                eb.WithColor(Discord.Color.Red);
                eb.AddField("Error:", "Was not able to Send the File");
                await Context.Channel.SendMessageAsync(string.Empty, false, eb.Build());
                Console.WriteLine(e.Message);
            }
            return;
        }
        public class db_image_get_class
        {
            public string file_ext;
            public string file_url;
            public string tag_string;
            public string tag_string_artist;
            public string tag_string_character;
        }

    }
}
