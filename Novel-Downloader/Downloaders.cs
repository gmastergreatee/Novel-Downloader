using System.Linq;
using Core.Models;
using System.Collections.Generic;

namespace Novel_Downloader
{
    public class Downloaders
    {
        private readonly IEnumerable<IDownloader> _downloaderList = new List<IDownloader>
        {
            new Webnovel.Webnovel(),
        };

        public static IDownloader GetValidDownloader(string url)
        {
            return (new Downloaders())._downloaderList.FirstOrDefault(itm => itm.UrlMatch(url));
        }
    }
}
