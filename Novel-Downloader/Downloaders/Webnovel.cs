using Novel_Downloader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Novel_Downloader.Downloaders
{
    public class Webnovel : IDownloader
    {
        HttpClient httpClient;
        string csrfToken = "";

        public Webnovel()
        {
            ResetClient();
        }

        public event EventHandler<NovelInfo> OnNovelInfoFetchSuccess;
        public event EventHandler<Exception> OnNovelInfoFetchError;

        public event EventHandler<List<ChapterInfo>> OnChapterListFetchSuccess;
        public event EventHandler<Exception> OnChapterListFetchError;

        public event EventHandler<ChapterData> OnChapterDataFetchSuccess;
        public event EventHandler<ChapterDataFetchError> OnChapterDataFetchError;

        public ChapterData GetChapterData(string chapterUrl)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ChapterInfo> GetChapterList(string novelUrl)
        {
            throw new NotImplementedException();
        }

        public void GetNovelInfoAsync(string novelUrl)
        {
            new Task(() =>
            {
                OnNovelInfoFetchSuccess?.Invoke(this, new NovelInfo());
            }).Start();
        }

        public void ResetClient()
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            }

            httpClient = new HttpClient();
        }

        public bool UrlMatch(string novelUrl)
        {
            return novelUrl.ToLower().StartsWith("https://www.webnovel.com/book/");
        }
    }
}
