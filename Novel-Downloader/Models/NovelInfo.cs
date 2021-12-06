namespace Novel_Downloader.Models
{
    public class NovelInfo
    {
        public string DownloaderId { get; set; }
        public string UniqueId { get; set; }
        public string NovelUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int ChapterCount { get; set; }

        public NovelInfo Copy()
        {
            return new NovelInfo()
            {
                UniqueId = UniqueId,
                NovelUrl = NovelUrl,
                ImageUrl = ImageUrl,
                Title = Title,
                Author = Author,
                ChapterCount = ChapterCount,
            };
        }
    }
}
