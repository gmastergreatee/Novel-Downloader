using System;
using System.IO;
using System.Net;
using AngleSharp;
using System.Linq;
using Core.Models;
using AngleSharp.Dom;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Webnovel
{
    public class Webnovel : IDownloader
    {
        string uuid = "";
        string csrfToken = "";
        NovelInfo novelInfo = null;
        Models.BookInfoResponse.BookInfoResponse bookInfoResp = null;

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

                Models.ChapterInfoResponse.ChapterInfoResponse chapterDataResp = null;

                try
                {
                    chapterDataResp = DeserializeJson<Models.ChapterInfoResponse.ChapterInfoResponse>(html);
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

                    #region Get Image URL
                    retThis.ImageUrl = "https://img.webnovel.com/bookcover/" + retThis.UniqueId + "/600/600.jpg";
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
                        bookInfoResp = DeserializeJson<Models.BookInfoResponse.BookInfoResponse>(html);
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

        public void GenerateDocument(string targetPath)
        {
            OnLog?.Invoke(this, "Generating EPUB file...");

            var epubFileName = novelInfo.Author + " - " + novelInfo.Title + ".epub";
            var invalidPathChars = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToList();
            foreach (var ch in invalidPathChars)
            {
                epubFileName = epubFileName.Replace(ch.ToString(), "");
            }
            var epubFilePath = Path.Combine(targetPath, epubFileName);

            EpubGenerator.Ebook book = new EpubGenerator.Ebook()
            {
                Title = novelInfo.Title,
                Author = novelInfo.Author,
            };

            byte[] imageBytes = null;
            if (File.Exists(Path.Combine(targetPath, "data", "image.jpg")))
            {
                imageBytes = File.ReadAllBytes(Path.Combine(targetPath, "data", "image.jpg"));
                var fData = new EpubGenerator.FileData()
                {
                    FileName = "image.jpeg",
                    FileType = EpubGenerator.EnumFileType.JPEG,
                    IsCoverImage = true,
                };
                fData.PutBytes(imageBytes);
                book.AddFile(fData);
            }

            {
                var styleSheet = new EpubGenerator.FileData()
                {
                    FileName = "style.css",
                    FileType = EpubGenerator.EnumFileType.CSS,
                };
                styleSheet.PutString("pirate{display:none}");
                book.AddFile(styleSheet);
            }
            var cssInclude = "<link rel=\"stylesheet\" href=\"../css/style.css\" >";

            var count = 1;
            foreach (var itm in bookInfoResp.data.volumeItems)
            {
                foreach (var itm2 in itm.chapterItems)
                {
                    book.ChapterDatas.Add(new EpubGenerator.ChapterData()
                    {
                        VolumeId = itm.volumeId,
                        VolumeName = itm.volumeName,
                        ChapterName = itm2.chapterName,
                        ChapterContent = cssInclude + itm2.content.Replace("\r\n", "<br>").Replace("\n", "<br>"),
                    });
                    count++;
                }
            }

            book.OnLog += (sender, log) =>
            {
                OnLog?.Invoke(this, log);
            };

            book.Write(epubFilePath);
            OnLog?.Invoke(this, "File saved to \"" + epubFilePath + "\"");
        }

        T DeserializeJson<T>(string Json)
        {
            var JavaScriptSerializer = new JavaScriptSerializer();
            return JavaScriptSerializer.Deserialize<T>(Json);
        }
    }
}
