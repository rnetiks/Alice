using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Alice.DataFetcher.MangaSources
{
    public class HentaiFoxSource : IMangaSource
    {
        public const string GalleryUrl = "https://hentaifox.com/gallery/{0}";

        public bool IsValidUrl(string url)
        {
            const string validRegex = @"^(?:https?://)?hentaifox\.com/(?:gallery|g)/\d+.*$";
            return Regex.IsMatch(url, validRegex, RegexOptions.IgnoreCase);
        }

        public static async Task<MangaInfo?> GetMangaAsync(int id)
        {
            var request = (HttpWebRequest) WebRequest.Create(string.Format(GalleryUrl, id));
            request.Method = "GET";
            request.UserAgent = "AliceBot";

            using var response = (HttpWebResponse) await request.GetResponseAsync();
            await using var stream = response.GetResponseStream();
            if (stream == null)
                return null;
            
            using var reader = new StreamReader(stream);

            var manga = new MangaInfo
            {
                Url = $"http://hentaifox.com/gallery/{id}",
                IsNsfw = true
            };

            string? imagesUrl = null;
            int? pages = null;
            
            // Get base url for all the pages
            string line;
            while (imagesUrl == null && (line = await reader.ReadLineAsync()) != null)
            {
                var index = line.IndexOf("<div class=\"cover\">", StringComparison.Ordinal);
                if (index >= 0)
                {
                    line = await reader.ReadLineAsync();
                    const string imageRegex = @".+src=""\W*([a-z0-9-\.]+(?:\/\d+)+).*"".+";
                    var match = Regex.Match(line, imageRegex, RegexOptions.IgnoreCase);
                    if (!match.Success)
                        return null;
                    imagesUrl = "https://" + match.Groups[1].Value + "/{0}";
                }
            }

            // Get Manga Name
            while (manga.Title == null && (line = await reader.ReadLineAsync()) != null)
            {
                var index = line.IndexOf("<h1>", StringComparison.Ordinal);
                if (index >= 0)
                    manga.Title = line.Substring(index + 4, line.Length - index - 4 - 5);
            }
            
            // Get Tags
            while (manga.Tags == null && (line = await reader.ReadLineAsync()) != null)
            {
                var index = line.IndexOf("Tags: ", StringComparison.Ordinal);
                if (index >= 0)
                {
                    const string tagsRegex = @"tag"">([\w\s-_]+?)<\/span";
                    var matches = Regex.Matches(line, tagsRegex, RegexOptions.IgnoreCase);

                    manga.Tags = new string[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                    {
                        manga.Tags[i] = matches[i].Groups[1].Value;
                    }
                }
            }
            
            // Get Artists
            while (manga.Artists == null && (line = await reader.ReadLineAsync()) != null)
            {
                var index = line.IndexOf("Artists: ", StringComparison.Ordinal);
                if (index >= 0)
                {
                    const string artistsRegex = @"tag"">([\w\s-_]+?)<\/span";
                    var matches = Regex.Matches(line, artistsRegex, RegexOptions.IgnoreCase);

                    manga.Artists = new string[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                    {
                        manga.Artists[i] = matches[i].Groups[1].Value;
                    }
                }
            }
            
            // Get Languages
            while (manga.Languages == null && (line = await reader.ReadLineAsync()) != null)
            {
                var index = line.IndexOf("Language: ", StringComparison.Ordinal);
                if (index >= 0)
                {
                    const string languagesRegex = @"tag"">([\w\s-_]+?)<\/span";
                    var matches = Regex.Matches(line, languagesRegex, RegexOptions.IgnoreCase);

                    manga.Languages = new string[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                    {
                        manga.Languages[i] = matches[i].Groups[1].Value;
                    }
                }
            }
            
            // Get Category
            while (manga.Categories == null && (line = await reader.ReadLineAsync()) != null)
            {
                var index = line.IndexOf("Category: ", StringComparison.Ordinal);
                if (index >= 0)
                {
                    const string categoryRegex = @"tag"">([\w\s-_]+?)<\/span";
                    var matches = Regex.Matches(line, categoryRegex, RegexOptions.IgnoreCase);

                    manga.Categories = new string[matches.Count];
                    for (int i = 0; i < matches.Count; i++)
                        manga.Categories[i] = matches[i].Groups[1].Value;
                }
            }

            // Get number of pages
            while (pages == null && (line = await reader.ReadLineAsync()) != null)
            {
                int index = line.IndexOf("<span class=\"pages\">Pages: ", StringComparison.Ordinal);
                if (index >= 0)
                {
                    int max = index + 27;
                    for (; max < line.Length; max++)
                        if (!char.IsNumber(line[max]))
                            break;

                    int.TryParse(line.Substring(index + 27, max - index - 27), out int pageCount);
                    if (pageCount == 0)
                        return null;
                    pages = pageCount;
                }
            }
            
            if (pages == null)
                return null;
            
            manga.Cover = string.Format(imagesUrl, "cover.jpg");
            
            manga.Pages = new string[pages.Value];
            for (int i = 0; i < pages; i++)
                manga.Pages[i] = string.Format(imagesUrl, i + 1 + ".jpg");

            return manga;
        }
    }
}