﻿namespace Core.Models
{
    public class NovelInfo
    {
        public string DownloaderId { get; set; }
        public string UniqueId { get; set; }
        public string NovelUrl { get; set; }
        public string ThumbUrl { get; set; }
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
                ThumbUrl = ThumbUrl,
                ImageUrl = ImageUrl,
                Title = Title,
                Author = Author,
                ChapterCount = ChapterCount,
            };
        }
    }
}
