using System.Collections.Generic;

namespace Novel_Downloader.Downloaders.Models.Webnovel.ChapterInfoResponse
{
    public class ChapterInfo
    {
        public string chapterName { get; set; }
        public List<Content> contents { get; set; }
    }
}
