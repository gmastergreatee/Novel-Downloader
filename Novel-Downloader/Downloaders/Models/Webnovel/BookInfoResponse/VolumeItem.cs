using System.Collections.Generic;

namespace Novel_Downloader.Downloaders.Models.Webnovel.BookInfoResponse
{
    public class VolumeItem
    {
        public int volumeId { get; set; }
        public string volumeName { get; set; }
        public List<ChapterItem> chapterItems { get; set; }
    }
}
