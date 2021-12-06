using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novel_Downloader.Downloaders.Models.Webnovel.BookInfoResponse
{
    public class VolumeItem
    {
        public List<ChapterItem> chapterItems { get; set; }
        public string volumeName { get; set; }
    }
}
