using JetBrains.Annotations;

namespace Alice.DataFetcher
{
    public struct MangaInfo
    {
        [CanBeNull] public string Cover { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string[] Pages { get; set; }
        public bool IsNsfw { get; set; }
        public string[] Tags { get; set; }
        public string[] Artists { get; set; }
        public string[] Languages { get; set; }
        public string[] Categories { get; set; }

        public int PageCount => Pages.Length;
    }
}