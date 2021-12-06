using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webnovel.Models.BookInfoResponse
{
    public class ChapterItem
    {
        public string chapterId { get; set; }
        public string chapterName { get; set; }
        public string content { get; set; } = "";
    }
}
