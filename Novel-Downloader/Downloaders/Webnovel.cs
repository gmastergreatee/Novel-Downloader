using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Novel_Downloader.Models;
using System.Collections.Generic;
using CsQuery;

namespace Novel_Downloader.Downloaders
{
    public class Webnovel : IDownloader
    {
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

                    CQ dom = html;

                    var retThis = new NovelInfo();

                    #region Get Image URL
                    try
                    {
                        var imageObj = dom["head meta[property='og:image']"].FirstOrDefault();
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

                    var titleObj = dom["head title"].FirstOrDefault();
                    if (titleObj != null)
                    {
                        var stuff = titleObj.InnerText.Trim().Split('-');
                        retThis.Title = stuff[0].Trim().Substring(5);
                        retThis.Author = stuff[1].Trim();
                    }
                    else
                        throw new Exception("Can't get Novel-Title & Author Info");

                    #endregion

                    #region Get UniqueId & NovelUrl

                    var uIdObj = dom["head meta[property='og:url']"].FirstOrDefault();
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

                    var chapObj = dom["body [data-report-bid=" + retThis.UniqueId + "]:first "].FirstOrDefault();
                    if (chapObj == null)
                        throw new Exception("Error getting chapter count");
                    chapObj = ((CQ)chapObj.ParentNode.InnerHTML)["strong:first span"].FirstOrDefault();
                    if (chapObj == null)
                        throw new Exception("Error getting chapter count -> 2");
                    var chapterText = chapObj.InnerText.Trim();
                    retThis.ChapterCount = Convert.ToInt32(chapterText.Substring(0, chapterText.IndexOf(" ")));

                    #endregion

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
        }

        public bool UrlMatch(string novelUrl)
        {
            return novelUrl.ToLower().StartsWith("https://www.webnovel.com/book/");
        }
    }
}
