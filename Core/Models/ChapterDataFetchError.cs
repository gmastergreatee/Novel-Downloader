using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class ChapterDataFetchError
    {
        public ChapterInfo ChapterInfo { get; set; }
        public Exception Exception { get; set; }
    }
}
