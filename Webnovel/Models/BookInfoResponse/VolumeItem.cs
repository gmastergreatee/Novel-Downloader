using System.Collections.Generic;

namespace Webnovel.Models.BookInfoResponse
{
    public class VolumeItem
    {
        public int volumeId { get; set; }
        public string volumeName { get; set; }
        public List<ChapterItem> chapterItems { get; set; }
    }
}
