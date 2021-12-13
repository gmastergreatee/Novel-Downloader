﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Core.Models.Library
{
    public class LibNovelInfo
    {
        public string URL { get; set; } = "";
        public string EpubFilePath { get; set; } = "";
        public string DataDirPath { get; set; } = "";
        public bool CheckForUpdates { get; set; } = true;

        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public int ChapterCount { get; set; } = 0;
        public int DownloadedTill { get; set; } = 0;
        public string Description { get; set; } = "";
    }
}