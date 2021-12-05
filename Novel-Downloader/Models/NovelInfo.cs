using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novel_Downloader.Models
{
    public class NovelInfo
    {
        public string UniqueId { get; set; }
        public string NovelUrl { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int ChapterCount { get; set; }
    }
}
