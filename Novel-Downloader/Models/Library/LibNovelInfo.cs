using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Novel_Downloader.Models.Library
{
    public class LibNovelInfo
    {
        public string URL { get; set; } = "";
        public string EpubFilePath { get; set; } = "";
        public string DataDirPath { get; set; } = "";

        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int ChapterCount { get; set; } = 0;
        public string CurrentChapter { get; set; } = "";
        public string LatestChapter { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
