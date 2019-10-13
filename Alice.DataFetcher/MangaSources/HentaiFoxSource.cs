using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Alice.DataFetcher.MangaSources
{
    public class HentaiFoxSource : IMangaSource
    {
        public const string GalleryUrl = "https://hentaifox.com/gallery/{0}";

        public bool ValidUrl(string url)
        {
            const string validRegex = @"^(?:https?://)?hentaifox\.com/(?:gallery|g)/\d+.+$";
            return Regex.IsMatch(url, validRegex, RegexOptions.IgnoreCase);
        }

        public static async IAsyncEnumerable<string> GetPages(int id)
        {
            var request = (HttpWebRequest) WebRequest.Create(string.Format(GalleryUrl, id));
            request.Method = "GET";
            request.UserAgent = "AliceBot";

            using var response = (HttpWebResponse) await request.GetResponseAsync();
            await using var stream = response.GetResponseStream();
            if (stream == null)
                yield break;
            
            using var reader = new StreamReader(stream);

            string? imagesUrl = null;
            int? pages = null;
            
            string line;
            while ((line = await reader.ReadLineAsync()) != null && imagesUrl == null)
            {
                var index = line.IndexOf("<div class=\"cover\">", StringComparison.InvariantCulture);
                if (index >= 0)
                {
                    line = await reader.ReadLineAsync();
                    const string imageUrl = @".+src=""\W*([a-z0-9-\.]+(?:\/\d+)+).*"".+";
                    var match = Regex.Match(line, imageUrl, RegexOptions.IgnoreCase);
                    if (!match.Success)
                        yield break;
                    imagesUrl = "https://" + match.Groups[1].Value + "/{0}";
                }
            }
            
            while ((line = await reader.ReadLineAsync()) != null && pages == null)
            {
                int index = line.IndexOf("<span class=\"pages\">Pages: ", StringComparison.InvariantCulture);
                if (index >= 0)
                {
                    int max = index + 27;
                    for (; max < line.Length; max++)
                        if (!char.IsNumber(line[max]))
                            break;

                    int.TryParse(line.Substring(index + 27, max - index - 27), out int pageCount);
                    if (pageCount == 0)
                        yield break;
                    pages = pageCount;
                }
            }

            yield return string.Format(imagesUrl, "cover.jpg");
            
            for (int i = 1; i <= pages; i++)
            {
                yield return string.Format(imagesUrl, i + ".jpg");
            }
        }
    }
}