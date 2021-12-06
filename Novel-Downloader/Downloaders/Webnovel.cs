using System;
using System.IO;
using System.Net;
using AngleSharp;
using System.Linq;
using AngleSharp.Dom;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Novel_Downloader.Models;
using System.Collections.Generic;

namespace Novel_Downloader.Downloaders
{
    public class Webnovel : IDownloader
    {
        string uuid = "";
        string csrfToken = "";
        NovelInfo novelInfo = null;
        Models.Webnovel.BookInfoResponse.BookInfoResponse bookInfoResp = null;

        public Webnovel()
        {
            ResetClient();
        }

        public event EventHandler<string> OnLog;

        public event EventHandler<NovelInfo> OnNovelInfoFetchSuccess;
        public event EventHandler<Exception> OnNovelInfoFetchError;

        public event EventHandler<List<ChapterInfo>> OnChapterListFetchSuccess;
        public event EventHandler<Exception> OnChapterListFetchError;

        public event EventHandler<ChapterData> OnChapterDataFetchSuccess;
        public event EventHandler<ChapterDataFetchError> OnChapterDataFetchError;

        public void FetchChapterData(ChapterInfo chapterInfo)
        {
            var chapterData = new ChapterData();

            try
            {
                var html = "";
                try
                {
                    var req = WebRequest.Create("https://www.webnovel.com/go/pcm/chapter/getContent?_csrfToken=" + csrfToken + "&bookId=" + bookInfoResp.data.bookInfo.bookId + "&chapterId=" + chapterInfo.ChapterUrl);
                    req.Method = WebRequestMethods.Http.Get;
                    req.Headers.Add("cookie", "_csrfToken=" + csrfToken + "; webnovel_uuid=" + uuid);
                    var resp = req.GetResponse();
                    using (var textReader = new StreamReader(resp.GetResponseStream()))
                    {
                        html = textReader.ReadToEnd();
                    }
                }
                catch
                {
                    throw new Exception("Error fetching chapter-data");
                }

                Models.Webnovel.ChapterInfoResponse.ChapterInfoResponse chapterDataResp = null;

                try
                {
                    chapterDataResp = JsonConvert.DeserializeObject<Models.Webnovel.ChapterInfoResponse.ChapterInfoResponse>(html);
                }
                catch
                {
                    throw new Exception("Error parsing chapter-data");
                }

                chapterData.Index = chapterInfo.Index;
                chapterData.Title = chapterDataResp.data.chapterInfo.chapterName;
                chapterData.Content = "";
                var lastContent = chapterDataResp.data.chapterInfo.contents.LastOrDefault();
                foreach (var itm in chapterDataResp.data.chapterInfo.contents)
                {
                    chapterData.Content += itm.content;
                    if (itm != lastContent)
                        chapterData.Content += Environment.NewLine;
                }

                var chapterItem_BookInfoResponse = bookInfoResp.data.volumeItems.SelectMany(i => i.chapterItems);
                var currentItem = chapterItem_BookInfoResponse.FirstOrDefault(i => i.chapterId == chapterInfo.ChapterUrl);
                if (currentItem != null)
                {
                    currentItem.content = chapterData.Content;
                }
                else
                    throw new Exception("Data redundancy error. This shouldn't be possible. Please file an issue");
            }
            catch (Exception ex)
            {
                OnChapterDataFetchError?.Invoke(this, new ChapterDataFetchError()
                {
                    ChapterInfo = chapterInfo,
                    Exception = ex,
                });
            }
            OnChapterDataFetchSuccess?.Invoke(this, chapterData);
        }

        public void FetchChapterList()
        {
            var count = 0;
            var chapters = new List<ChapterInfo>();
            foreach (var itm in bookInfoResp.data.volumeItems)
            {
                foreach (var itm2 in itm.chapterItems)
                {
                    chapters.Add(new ChapterInfo()
                    {
                        Index = ++count,
                        VolumeId = itm.volumeId,
                        VolumeName = itm.volumeName,
                        ChapterUrl = itm2.chapterId,
                        ChapterName = itm2.chapterName,
                    });
                }
            }

            OnChapterListFetchSuccess?.Invoke(this, chapters);
            //OnChapterListFetchError(this, null); // isn't needed
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

                    #region Set csrfToken & uuid
                    try
                    {
                        csrfToken = resp.Headers.GetValues("Set-Cookie").FirstOrDefault(i => i.ToLower().Contains("csrftoken")).Replace("; ", ";").Split(';').FirstOrDefault(i => i.ToLower().Contains("csrftoken")).Split('=')[1];
                        uuid = resp.Headers.GetValues("Set-Cookie").FirstOrDefault(i => i.ToLower().Contains("webnovel_uuid")).Replace("; ", ";").Split(';').FirstOrDefault(i => i.ToLower().Contains("webnovel_uuid")).Split('=')[1];
                    }
                    catch
                    {
                        throw new Exception("Error reading \"_csrfToken\"");
                    }
                    #endregion

                    var dom = await BrowsingContext.New(Configuration.Default).OpenAsync(r => r.Content(html));

                    var retThis = new NovelInfo
                    {
                        DownloaderId = "webnovel.com"
                    };

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

                    #region Get Title, Author & Chapters

                    try
                    {
                        OnLog?.Invoke(this, "Fetching chapter list");
                        req = WebRequest.Create("https://www.webnovel.com/go/pcm/chapter/get-chapter-list?_csrfToken=" + csrfToken + "&bookId=" + retThis.UniqueId);
                        req.Method = WebRequestMethods.Http.Get;
                        req.Headers.Add("cookie", "_csrfToken=" + csrfToken + "; webnovel_uuid=" + uuid);
                        html = "";
                        resp = req.GetResponse();
                        using (var textReader = new StreamReader(resp.GetResponseStream()))
                        {
                            html = textReader.ReadToEnd();
                        }
                    }
                    catch
                    {
                        throw new Exception("Error fetching chapter list");
                    }

                    try
                    {
                        bookInfoResp = JsonConvert.DeserializeObject<Models.Webnovel.BookInfoResponse.BookInfoResponse>(html);
                        if (bookInfoResp.code != 0)
                            throw new Exception("Invalid chapter list code returned");
                        retThis.Title = bookInfoResp.data.bookInfo.bookName;
                        retThis.Author = bookInfoResp.data.bookInfo.authorName;
                        retThis.ChapterCount = bookInfoResp.data.bookInfo.totalChapterNum;
                        OnLog?.Invoke(this, "Done");
                    }
                    catch
                    {
                        throw new Exception("Error parsing chapter list");
                    }

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
            uuid = "";
            csrfToken = "";
            novelInfo = null;
            bookInfoResp = null;
        }

        public bool UrlMatch(string novelUrl)
        {
            return novelUrl.ToLower().StartsWith("https://www.webnovel.com/book/");
        }

        public void GenerateEPUB(string targetPath)
        {
            throw new NotImplementedException();
        }
    }
}
