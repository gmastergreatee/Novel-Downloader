using System.Collections.Generic;

namespace Webnovel.Models.BookInfoResponse
{
    public class GetChapterData
    {
        public BookInfo bookInfo { get; set; }
        public List<VolumeItem> volumeItems { get; set; }
    }
}
