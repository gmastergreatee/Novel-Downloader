using System;
using System.Linq;
using System.Text;
using Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Novel_Downloader
{
    public class Downloaders
    {
        static readonly IEnumerable<IDownloader> downloaderList = new List<IDownloader>
        {
            new Webnovel.Webnovel(),
        };

        public static IDownloader GetValidDownloader(string URL)
        {
            IDownloader currentDownloader = null;
            foreach (var itm in downloaderList)
            {
                if (itm.UrlMatch(URL))
                {
                    currentDownloader = itm;
                    break;
                }
            }
            return currentDownloader;
        }
    }
}
