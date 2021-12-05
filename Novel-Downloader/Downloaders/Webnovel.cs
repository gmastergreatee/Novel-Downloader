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
using AngleSharp.Dom;

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

        public void FetchChapterData(ChapterInfo chapterInfo)
        {
            OnChapterDataFetchSuccess(this, null);
            OnChapterDataFetchError(this, null);
            throw new NotImplementedException();
        }

        public void FetchChapterList()
        {
            OnChapterListFetchSuccess(this, null);
            OnChapterListFetchError(this, null);
            throw new NotImplementedException();
        }

        public void FetchNovelInfo(string novelUrl)
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
                    IElement titleObj = null;
                    try
                    {
                        titleObj = dom.QuerySelectorAll("head title").FirstOrDefault();
                    }
                    catch
                    {
                        throw new Exception("Can't get Novel-Title & Author Info, please check the URL entered");
                    }

                    string[] stuff = null;

                    try
                    {
                        stuff = titleObj.TextContent.Trim().Split('-');
                    }
                    catch
                    {
                        throw new Exception("Can't get Novel-Title & Author Info, please check the URL entered -> ECODE 2");
                    }

                    if (stuff.Length != 3)
                        throw new Exception("Can't get Novel-Title & Author Info, please check the URL entered -> ECODE 3");
                    retThis.Title = stuff[0].Trim().Substring(5);
                    retThis.Author = stuff[1].Trim();

                    #endregion

                    #region Get UniqueId & NovelUrl
                    IElement uIdObj = null;
                    try
                    {
                        uIdObj = dom.QuerySelectorAll("head meta[property='og:url']").FirstOrDefault();
                    }
                    catch
                    {
                        throw new Exception("Error fetching NovelURL & UniqueId");
                    }

                    string url = "";
                    try
                    {
                        url = uIdObj.GetAttribute("content");
                    }
                    catch
                    {
                        throw new Exception("Can't get Novel URL");
                    }

                    if (string.IsNullOrWhiteSpace(url))
                        throw new Exception("Error fetching UniqueId");

                    retThis.NovelUrl = url;
                    retThis.UniqueId = url.Substring(url.LastIndexOf("_") + 1);

                    #endregion

                    #region Get Chapter Count
                    IElement chapObj = null;
                    // -- finding element close to chapter count
                    try
                    {
                        chapObj = dom.QuerySelectorAll("body [data-report-bid=" + retThis.UniqueId + "]").FirstOrDefault();
                    }
                    catch
                    {
                        throw new Exception("Error getting chapter count");
                    }

                    // -- navigating to chapter count text
                    try
                    {
                        chapObj = chapObj.ParentElement.QuerySelectorAll("strong span").FirstOrDefault();
                    }
                    catch
                    {
                        throw new Exception("Error getting chapter count -> ECODE 2");
                    }

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
