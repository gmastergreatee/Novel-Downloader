using System.Collections.Generic;

namespace Webnovel.Models.ChapterInfoResponse
{
    public class ChapterInfo
    {
        public string chapterName { get; set; }
        public List<Content> contents { get; set; }
    }
}
