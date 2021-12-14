using Core;
using System;
using System.IO;
using System.Net;
using AngleSharp;
using System.Linq;
using Core.Models;
using AngleSharp.Dom;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Webnovel
{
    public class Webnovel : IDownloader
    {
        private string _uuid = "";
        private string _csrfToken = "";
        private NovelInfo _novelInfo;
        private Models.BookInfoResponse.BookInfoResponse _bookInfoResp;

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

        #region Downloading related

        public void FetchChapterData(ChapterInfo chapterInfo)
        {
            var chapterData = new ChapterData();

            try
            {
                string html;
                try
                {
                    var req = WebRequest.Create("https://www.webnovel.com/go/pcm/chapter/getContent?_csrfToken=" + _csrfToken + "&bookId=" + _bookInfoResp.data.bookInfo.bookId + "&chapterId=" + chapterInfo.ChapterUrl);
                    req.Method = WebRequestMethods.Http.Get;
                    req.Headers.Add("cookie", "_csrfToken=" + _csrfToken + "; webnovel_uuid=" + _uuid);
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

                Models.ChapterInfoResponse.ChapterInfoResponse chapterDataResp;

                try
                {
                    chapterDataResp = JsonUtils.DeserializeJson<Models.ChapterInfoResponse.ChapterInfoResponse>(html);
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

                var chapterItemBookInfoResponse = _bookInfoResp.data.volumeItems.SelectMany(i => i.chapterItems);
                var currentItem = chapterItemBookInfoResponse.FirstOrDefault(i => i.chapterId == chapterInfo.ChapterUrl);
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
            foreach (var itm in _bookInfoResp.data.volumeItems)
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
            // ReSharper disable once AsyncVoidLambda
            new Task(async () =>
            {
                try
                {
                    var req = WebRequest.Create(novelUrl);
                    req.Method = WebRequestMethods.Http.Get;
                    var resp = await req.GetResponseAsync();
                    string html;
                    using (var textReader = new StreamReader(resp.GetResponseStream()))
                    {
                        html = await textReader.ReadToEndAsync();
                    }

                    #region Set csrfToken & uuid
                    try
                    {
                        _csrfToken = resp.Headers.GetValues("Set-Cookie").FirstOrDefault(i => i.ToLower().Contains("csrftoken")).Replace("; ", ";").Split(';').FirstOrDefault(i => i.ToLower().Contains("csrftoken")).Split('=')[1];
                        _uuid = resp.Headers.GetValues("Set-Cookie").FirstOrDefault(i => i.ToLower().Contains("webnovel_uuid")).Replace("; ", ";").Split(';').FirstOrDefault(i => i.ToLower().Contains("webnovel_uuid")).Split('=')[1];
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

                    #region Get UniqueId & NovelUrl
                    IElement uIdObj;
                    try
                    {
                        uIdObj = dom.QuerySelectorAll("head meta[property='og:url']").FirstOrDefault();
                    }
                    catch
                    {
                        throw new Exception("Error fetching NovelURL & UniqueId");
                    }

                    string url;
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

                    #region Get Image URL
                    retThis.ThumbUrl = "https://img.webnovel.com/bookcover/" + retThis.UniqueId + "/150/150.jpg";
                    retThis.ImageUrl = "https://img.webnovel.com/bookcover/" + retThis.UniqueId + "/600/600.jpg";
                    #endregion

                    #region Get Title, Author & Chapters

                    try
                    {
                        req = WebRequest.Create("https://www.webnovel.com/go/pcm/chapter/get-chapter-list?_csrfToken=" + _csrfToken + "&bookId=" + retThis.UniqueId);
                        req.Method = WebRequestMethods.Http.Get;
                        req.Headers.Add("cookie", "_csrfToken=" + _csrfToken + "; webnovel_uuid=" + _uuid);
                        html = "";
                        resp = await req.GetResponseAsync();
                        using (var textReader = new StreamReader(resp.GetResponseStream()))
                        {
                            html = await textReader.ReadToEndAsync();
                        }
                    }
                    catch
                    {
                        throw new Exception("Error fetching chapter list");
                    }

                    try
                    {
                        _bookInfoResp = JsonUtils.DeserializeJson<Models.BookInfoResponse.BookInfoResponse>(html);
                        if (_bookInfoResp.code != 0)
                            throw new Exception("Invalid chapter list code returned");
                        retThis.Title = _bookInfoResp.data.bookInfo.bookName;
                        retThis.Author = _bookInfoResp.data.bookInfo.authorName;
                        retThis.ChapterCount = _bookInfoResp.data.bookInfo.totalChapterNum;
                    }
                    catch
                    {
                        throw new Exception("Error parsing chapter list");
                    }

                    #endregion

                    _novelInfo = retThis.Copy();

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
            _uuid = "";
            _csrfToken = "";
            _novelInfo = null;
            _bookInfoResp = null;
        }

        public bool UrlMatch(string novelUrl)
        {
            return novelUrl.ToLower().StartsWith("https://www.webnovel.com/book/");
        }

        public void GenerateDocument(string targetDirPath, int numChapterFetched)
        {
            var info = JsonUtils.DeserializeJson<NovelInfo>(File.ReadAllText(Path.Combine(targetDirPath, "data", "info.json")));
            var list = JsonUtils.DeserializeJson<List<ChapterInfo>>(File.ReadAllText(Path.Combine(targetDirPath, "data", "list.json")));

            OnLog?.Invoke(this, "Generating EPUB file...");

            var book = new EpubGenerator.Ebook()
            {
                Title = info.Title,
                Author = info.Author,
            };

            if (File.Exists(Path.Combine(targetDirPath, "data", "image.jpg")))
            {
                var imageBytes = File.ReadAllBytes(Path.Combine(targetDirPath, "data", "image.jpg"));
                var fData = new EpubGenerator.FileData()
                {
                    FileName = "image.jpeg",
                    FileType = EpubGenerator.EnumFileType.JPEG,
                    IsCoverImage = true,
                };
                fData.PutBytes(imageBytes);
                book.AddFile(fData);
            }

            // adding stylesheet
            {
                var styleSheet = new EpubGenerator.FileData()
                {
                    FileName = "style.css",
                    FileType = EpubGenerator.EnumFileType.CSS,
                };
                styleSheet.PutString("pirate{display:none}");
                book.AddFile(styleSheet);
            }
            const string cssInclude = "<link rel=\"stylesheet\" href=\"../css/style.css\" >";

            var chaptersFetchedCounter = 0;
            foreach (var itm in list)
            {
                var chapterDataFilePath = Path.Combine(targetDirPath, "data", itm.Index + ".json");
                if (!File.Exists(chapterDataFilePath))
                {
                    OnLog?.Invoke(this, $"ERROR -> Data File not found - \"{itm.Index}.json\" - \"{info.Title}\" by \"{info.Author}\"");
                    continue;
                }

                ChapterData contentObj = null;
                try
                {
                    contentObj = JsonUtils.DeserializeJson<ChapterData>(File.ReadAllText(chapterDataFilePath));
                }
                catch
                {
                    OnLog?.Invoke(this, $"ERROR -> Corrupt/Invalid Data File - \"{itm.Index}.json\" - \"{info.Title}\" by \"{info.Author}\"");
                    continue;
                }

                if (++chaptersFetchedCounter >= numChapterFetched)
                    break;

                book.ChapterDatas.Add(new EpubGenerator.ChapterData()
                {
                    VolumeId = itm.VolumeId,
                    VolumeName = itm.VolumeName,
                    ChapterName = itm.ChapterName,
                    ChapterContent = cssInclude + contentObj.Content.Replace("\r\n", "<br>").Replace("\n", "<br>"),
                });
            }

            book.OnLog += (sender, log) =>
            {
                OnLog?.Invoke(this, log);
            };

            var epubFilePath = Path.Combine(targetDirPath, FileUtils.GetValidFileName(_novelInfo.Title, _novelInfo.Author) + ".epub");
            book.Write(epubFilePath);

            OnLog?.Invoke(this, "File saved to \"" + epubFilePath + "\"");
        }

        #endregion
    }
}
