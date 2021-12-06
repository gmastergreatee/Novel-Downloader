using System.Collections.Generic;

namespace Novel_Downloader.Downloaders.Models.Webnovel.BookInfoResponse
{
    public class GetChapterData
    {
        public BookInfo bookInfo { get; set; }
        public List<VolumeItem> volumeItems { get; set; }
    }
}
