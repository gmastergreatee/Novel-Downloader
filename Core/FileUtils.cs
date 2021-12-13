using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public static class FileUtils
    {
        public static string GetValidFileName(string title, string author)
        {
            var epubFileName = author + " - " + title;
            var invalidPathChars = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToList();

            return invalidPathChars.Aggregate(
                epubFileName,
                (current, ch) =>
                    current.Replace(ch.ToString(), "")
            );
        }
    }
}
