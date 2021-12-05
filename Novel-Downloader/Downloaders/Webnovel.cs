using System;
using System.IO;
using System.Net;
using AngleSharp;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Novel_Downloader.Models;
using System.Collections.Generic;

namespace Novel_Downloader.Downloaders
{
    public class Webnovel : IDownloader
    {
        string csrfToken = "";
        NovelInfo novelInfo = null;

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
            new Task(async () =>
            {
                try
                {
                    var req = WebRequest.Create(novelUrl);
                    req.Method = WebRequestMethods.Http.Get;
                    var resp = req.GetResponse();
                    var html = "";
                    using (var textReader = new StreamReader(resp.GetResponseStream()))
                    {
                        html = textReader.ReadToEnd();
                    }

                    #region Set csrfToken
                    try
                    {
                        csrfToken = resp.Headers.GetValues("Set-Cookie").FirstOrDefault(i => i.ToLower().Contains("csrftoken")).Replace("; ", ";").Split(';').FirstOrDefault(i => i.ToLower().Contains("csrftoken")).Split('=')[1];
                    }
                    catch
                    {
                        throw new Exception("Error reading \"_csrfToken\"");
                    }
                    #endregion

                    var dom = await BrowsingContext.New(Configuration.Default).OpenAsync(r => r.Content(html));

                    var retThis = new NovelInfo();

                    #region Get Image URL
                    try
                    {
                        var imageObj = dom.QuerySelectorAll("head meta[property='og:image']").FirstOrDefault();
                        if (imageObj != null)
                        {
                            var imgUrl = imageObj.GetAttribute("content");
                            if (!string.IsNullOrWhiteSpace(imgUrl))
                                retThis.ImageUrl = imgUrl;
                        }
                    }
                    catch { }
                    #endregion

                    #region Get Title and Author

                    var titleObj = dom.QuerySelectorAll("head title").FirstOrDefault();
                    if (titleObj != null)
                    {
                        var stuff = titleObj.TextContent.Trim().Split('-');
                        retThis.Title = stuff[0].Trim().Substring(5);
                        retThis.Author = stuff[1].Trim();
                    }
                    else
                        throw new Exception("Can't get Novel-Title & Author Info");

                    #endregion

                    #region Get UniqueId & NovelUrl

                    var uIdObj = dom.QuerySelectorAll("head meta[property='og:url']").FirstOrDefault();
                    if (uIdObj != null)
                    {
                        var url = uIdObj.GetAttribute("content");
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            retThis.NovelUrl = url;
                            retThis.UniqueId = url.Substring(url.LastIndexOf("_") + 1);
                        }
                        else throw new Exception("Error fetching UniqueId");
                    }

                    #endregion

                    #region Get Chapter Count

                    var chapObj = dom.QuerySelectorAll("body [data-report-bid=" + retThis.UniqueId + "]").FirstOrDefault();
                    if (chapObj == null)
                        throw new Exception("Error getting chapter count");
                    chapObj = chapObj.ParentElement.QuerySelectorAll("strong span").FirstOrDefault();
                    if (chapObj == null)
                        throw new Exception("Error getting chapter count -> 2");
                    var chapterText = chapObj.TextContent.Trim();
                    retThis.ChapterCount = Convert.ToInt32(chapterText.Substring(0, chapterText.IndexOf(" ")));

                    #endregion

                    novelInfo = retThis.Copy();

                    OnNovelInfoFetchSuccess?.Invoke(this, retThis);
                }
                catch (Exception ex)
                {
                    OnNovelInfoFetchError?.Invoke(this, ex);
                }
            }).Start();
        }

        public void ResetClient()
        {
            csrfToken = "";
            novelInfo = null;
        }

        public bool UrlMatch(string novelUrl)
        {
            return novelUrl.ToLower().StartsWith("https://www.webnovel.com/book/");
        }
    }
}
