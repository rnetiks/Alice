using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alice.DataFetcher.MangaSources;
using Discord.Commands;

namespace Alice.Discord.Commands
{
    public class NsfwMangaCommands : ModuleBase<SocketCommandContext>
    {
        [
            Command("hentaifox", RunMode = RunMode.Async),
            Summary("Creates manga reader for specified id"),
            RequireNsfw,
        ]
        public async Task<RuntimeResult> HentaiFox(int id)
        {
            var pages = new List<string>();
            await foreach (var item in HentaiFoxSource.GetPages(id)) 
                pages.Add(item);
            
            Console.WriteLine("Fetched manga {0}, pages: {1}", id, pages.Count);
            Console.WriteLine("Context.Channel {0} Context.Message.Channel {1}", Context.Channel.Id, Context.Message.Channel.Id);
            if (pages.Count == 0)
                return Result.Error("Cannot find any hentai with id {0}", id);

            var reader = new MangaReader(Context.Client, pages, Context.User.Id);
            await reader.Create(Context.Channel, string.Format(HentaiFoxSource.GalleryUrl, id));
            return Result.Successful;
        }
    }
}