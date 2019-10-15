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
            var mangaInfo = await HentaiFoxSource.GetMangaAsync(id);
            
            if (mangaInfo == null)
                return Result.Error("Could not download manga {0}", id);
            var manga = mangaInfo.Value;
            
            if (manga.PageCount == 0)
                return Result.Error("Could not extract any page from manga {0}", id);

            var reader = new MangaReader(Context.Client, manga, Context.User.Id);
            await reader.Create(Context.Channel, string.Format(HentaiFoxSource.GalleryUrl, id));
            return Result.Successful;
        }
    }
}