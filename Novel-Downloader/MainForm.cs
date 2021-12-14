using Core;
using System;
using System.IO;
using System.Linq;
using Core.Models;
using Core.Models.Library;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Novel_Downloader
{
    public partial class MainForm : Form
    {
        #region vars

        NovelInfo novelInfo { get; set; } = null;

        IDownloader currentDownloader { get; set; } = null;
        List<ChapterInfo> errorChapterInfos { get; set; } = null;
        bool stopFetchingChapterData { get; set; } = false;
        string TargetPath { get; set; } = "";
        bool grabbingChapters { get; set; } = false;

        #endregion

        public MainForm()
        {
            InitializeComponent();
            ExtraInit();
        }

        #region Downloader Events

        private void OnLog(object sender, string e)
        {
            Invoke(new Action(() =>
            {
                txtConsole.AppendText(e + Environment.NewLine);
            }));
        }

        // ------------------------------- Novel Information

        private void OnNovelInfoFetchSuccess(object sender, NovelInfo novelInfo)
        {
            this.novelInfo = novelInfo;
            Invoke(new Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(novelInfo.ThumbUrl))
                    pictureBox1.ImageLocation = novelInfo.ThumbUrl;

                lblTitle.Text = novelInfo.Title;
                lblAuthor.Text = novelInfo.Author;
                lblChapterCount.Text = novelInfo.ChapterCount.ToString();

                progDownload.Maximum = novelInfo.ChapterCount;
                txtConsole.AppendText("---------- Novel Found ----------" + Environment.NewLine);
                txtConsole.AppendText("Title\t\t: " + novelInfo.Title + Environment.NewLine);
                txtConsole.AppendText("Author\t\t: " + novelInfo.Author + Environment.NewLine);
                txtConsole.AppendText("Chapters\t: " + novelInfo.ChapterCount + Environment.NewLine);

                UnlockControls(novelInfo.ChapterCount > 0);
            }));
        }

        private void OnNovelInfoFetchError(object sender, Exception e)
        {
            Invoke(new Action(() =>
            {
                txtConsole.AppendText("ERROR -> Error fetching novel info." + Environment.NewLine + "---------- ERROR ----------" + Environment.NewLine + ParseExceptionMessage(e) + Environment.NewLine + "---------------------------" + Environment.NewLine);
                UnlockControls();
            }));
        }

        // ------------------------------- Chapter List

        private void OnChapterListFetchSuccess(object sender, List<ChapterInfo> e)
        {
            OnLog(sender, "Done");
            Invoke(new Action(() =>
            {
                progDownload.Maximum = e.Count;
            }));

            var chapterInfosToDownload = e.ToList();
            new Task(() =>
            {
                if (!string.IsNullOrWhiteSpace(novelInfo.ImageUrl))
                {
                    OnLog(sender, "Saving image...");
                    try
                    {
                        // saving thumb
                        if (pictureBox1.Image != null)
                        {
                            var thumbLoc = Path.Combine(TargetPath, "data", "thumb.jpg");
                            if (File.Exists(thumbLoc))
                                File.Delete(thumbLoc);
                            pictureBox1.Image.Save(thumbLoc);
                        }

                        // saving HQ cover-image
                        (new System.Net.WebClient()).DownloadFile(novelInfo.ImageUrl, Path.Combine(TargetPath, "data", "image.jpg"));
                    }
                    catch
                    {
                        OnLog(sender, "ERROR -> Error saving image file");
                    }
                }

                OnLog(sender, "Saving novel-info...");
                try
                {
                    File.WriteAllText(Path.Combine(TargetPath, "data", "info.json"), JsonUtils.SerializeJson(novelInfo));
                }
                catch
                {
                    OnLog(sender, "ERROR -> Error writing novel-info");
                    UnlockControls(true);
                    return;
                }

                OnLog(sender, "Saving chapter-list...");
                try
                {
                    File.WriteAllText(Path.Combine(TargetPath, "data", "list.json"), JsonUtils.SerializeJson(e));
                }
                catch
                {
                    OnLog(sender, "ERROR -> Error writing chapter-list");
                    UnlockControls(true);
                    return;
                }

                var count = 1;
                while (chapterInfosToDownload.Count > 0)
                {
                    foreach (var chapter in chapterInfosToDownload)
                    {
                        OnLog(
                            this,
                            "[" + count + "/" + e.Count + "] Downloading -> " +
                            (string.IsNullOrWhiteSpace(chapter.ChapterName) ? "..." : chapter.ChapterName)
                        );
                        currentDownloader.FetchChapterData(chapter);

                        if (stopFetchingChapterData)
                            break;

                        count++;
                    }
                    chapterInfosToDownload = errorChapterInfos.OrderBy(i => i.Index).ToList();
                    errorChapterInfos.Clear();

                    if (stopFetchingChapterData)
                        break;

                    var forceBreak = false;
                    if (chapterInfosToDownload.Count > 0)
                    {
                        Invoke(new Action(() =>
                        {
                            if (MessageBox.Show(chapterInfosToDownload.Count + " chapter/s weren't downloaded. Do you want to retry ?", "Query", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                            {
                                forceBreak = true;
                            }
                        }));
                    }

                    if (forceBreak)
                        break;
                }

                if (!stopFetchingChapterData)
                    OnLog(sender, "Download complete");
                else
                {
                    // hack for the no. of chapters fetched
                    count += 1;

                    stopFetchingChapterData = false;
                    OnLog(sender, "Download stopped");
                }

                OnLog(sender, (count - 1) + " chapters fetched");

                currentDownloader.GenerateDocument(TargetPath, count);

                Invoke(new Action(() =>
                {
                    UnlockControls(chapterInfosToDownload.Count <= 0);
                    progDownload.Value = 0;

                    btnAddToLibrary.Visible = true;
                    btnGrabChapters.Text = "Grab Chapters";
                    btnGrabChapters.Enabled = true;
                    grabbingChapters = false;
                }));
            }).Start();
        }

        private void OnChapterListFetchError(object sender, Exception e)
        {
            OnLog(sender, "ERROR -> Error fetching chapter list." + Environment.NewLine + "---------- ERROR ----------" + Environment.NewLine + ParseExceptionMessage(e) + Environment.NewLine);

            Invoke(new Action(() =>
            {
                UnlockControls(true);
                progDownload.Value = 0;
                stopFetchingChapterData = false;
                btnGrabChapters.Text = "Grab Chapters";
                btnGrabChapters.Enabled = true;
                grabbingChapters = false;
            }));
        }

        // ------------------------------- Chapter Data

        private void OnChapterDataFetchSuccess(object sender, ChapterData e)
        {
            try
            {
                File.WriteAllText(Path.Combine(TargetPath, "data", e.Index + ".json"), JsonUtils.SerializeJson(e));

                Invoke(new Action(() =>
                {
                    progDownload.Value += 1;
                }));
            }
            catch
            {
                OnLog(sender, "ERROR -> Error writing chapter-data");
            }
        }

        private void OnChapterDataFetchError(object sender, ChapterDataFetchError e)
        {
            OnLog(sender, "ERROR -> " + ParseExceptionMessage(e.Exception));
            var alreadyExist = errorChapterInfos.FirstOrDefault(i => i.ChapterUrl == e.ChapterInfo.ChapterUrl) != null;
            if (!alreadyExist)
                errorChapterInfos.Add(e.ChapterInfo);
        }

        // ------------------------------- Chapter Updates

        private void OnStartDownloadNovelUpdate(object sender, LibNovelInfo e)
        {
            OnLog(sender, $"UPDATE -> \"{e.Title}\" by \"{e.Author}\"...");

            IDownloader dwn = Downloaders.GetValidDownloader(e.URL);
            if (dwn == null)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("No matching downloaders found", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    OnLog(sender, "ERROR -> Oops! No matching downloaders found");
                }));
                return;
            }

            // add event handlers
            {
                dwn.OnLog += OnLog;

                NovelInfo tempNovelInfo = null;
                var targetPath = Path.GetDirectoryName(e.DataDirPath);
                var errChapterInfos = new List<ChapterInfo>();

                dwn.OnNovelInfoFetchSuccess += (a, b) =>
                {
                    OnLog(a, $"INFO -> Fetched - \"{e.Title}\" by \"{e.Author}\"");
                    tempNovelInfo = b;
                    OnLog(a, $"LIST -> Fetching - \"{e.Title}\" by \"{e.Author}\"");
                    dwn.FetchChapterList();
                };
                dwn.OnNovelInfoFetchError += (a, b) =>
                {
                    OnLog(a, $"ERROR -> Fetching info - \"{e.Title}\" by \"{e.Author}\"" + Environment.NewLine + "---------- ERROR ----------" + Environment.NewLine + ParseExceptionMessage(b) + Environment.NewLine + "---------------------------");
                    libraryUserControl1.NovelUpdated(e);
                };
                dwn.OnChapterListFetchSuccess += (a, b) =>
                {
                    OnLog(a, $"LIST -> Fetched - \"{e.Title}\" by \"{e.Author}\"");

                    var alreadyDownloadedChapterCount = 0;
                    var _chapterInfosToDownload = new List<ChapterInfo>();
                    foreach (var chap in b)
                    {
                        if (!File.Exists(Path.Combine(targetPath, "data", chap.Index + ".json")))
                            _chapterInfosToDownload.Add(chap);
                        else
                            alreadyDownloadedChapterCount++;
                    }
                    var chapterInfosToDownload = _chapterInfosToDownload.ToList();

                    new Task(() =>
                    {
                        if (!string.IsNullOrWhiteSpace(tempNovelInfo.ImageUrl))
                        {
                            var thumbLoc = Path.Combine(targetPath, "data", "thumb.jpg");
                            var imageLoc = Path.Combine(targetPath, "data", "image.jpg");
                            if (!File.Exists(thumbLoc) || !File.Exists(imageLoc))
                            {
                                OnLog(a, "Saving image...");
                                try
                                {
                                    // saving thumb
                                    if (!File.Exists(thumbLoc) && !string.IsNullOrWhiteSpace(tempNovelInfo.ThumbUrl))
                                    {
                                        (new System.Net.WebClient()).DownloadFile(tempNovelInfo.ThumbUrl, thumbLoc);
                                    }

                                    // saving HQ cover-image
                                    if (!File.Exists(imageLoc))
                                        (new System.Net.WebClient()).DownloadFile(tempNovelInfo.ImageUrl, imageLoc);
                                }
                                catch
                                {
                                    OnLog(a, "ERROR -> Error saving image file");
                                }
                            }
                        }

                        if (e.ChapterCount < tempNovelInfo.ChapterCount)
                        {
                            e.ChapterCount = tempNovelInfo.ChapterCount;
                            OnLog(a, "Saving novel-info...");
                            try
                            {
                                File.WriteAllText(Path.Combine(targetPath, "data", "info.json"), JsonUtils.SerializeJson(e));
                            }
                            catch
                            {
                                OnLog(a, "ERROR -> Error writing novel-info");
                                return;
                            }

                            OnLog(a, "Saving chapter-list...");
                            try
                            {
                                File.WriteAllText(Path.Combine(targetPath, "data", "list.json"), JsonUtils.SerializeJson(b));
                            }
                            catch
                            {
                                OnLog(a, "ERROR -> Error writing chapter-list");
                                return;
                            }
                        }

                        var count = 1;
                        var retries = 0;
                        while (retries < 2)
                        {
                            if (retries > 0)
                                OnLog(a, "Retrying...");

                            foreach (var chapter in chapterInfosToDownload)
                            {
                                // skip chapter if already downloaded
                                if (File.Exists(Path.Combine(targetPath, "data", chapter.Index + ".json")))
                                    continue;

                                OnLog(
                                    a,
                                    "[" + count + "/" + _chapterInfosToDownload.Count + "] Downloading -> " +
                                    (string.IsNullOrWhiteSpace(chapter.ChapterName) ? "..." : chapter.ChapterName)
                                );
                                dwn.FetchChapterData(chapter);

                                count++;
                            }
                            chapterInfosToDownload = errChapterInfos.OrderBy(i => i.Index).ToList();
                            errChapterInfos.Clear();

                            if (chapterInfosToDownload.Count > 0)
                            {
                                ++retries;
                                OnLog(a, chapterInfosToDownload.Count + " chapter/s weren't downloaded." + (retries > 2 ? " Please re-update." : ""));
                            }
                            else
                                break;
                        }

                        if (chapterInfosToDownload.Count > 0)
                        {
                            count++;
                            OnLog(a, "Download partially complete");
                        }
                        else
                            OnLog(a, "Download complete");

                        OnLog(a, (count - 1) + " chapters fetched");

                        dwn.GenerateDocument(targetPath, alreadyDownloadedChapterCount + count);
                        e.DownloadedTill += (count - 1);
                        libraryUserControl1.NovelUpdated(e);
                    }).Start();
                };
                dwn.OnChapterListFetchError += (a, b) =>
                {
                    OnLog(a, "ERROR -> Error fetching chapter list." + Environment.NewLine + "---------- ERROR ----------" + Environment.NewLine + ParseExceptionMessage(b));
                    libraryUserControl1.NovelUpdated(e);
                };
                dwn.OnChapterDataFetchSuccess += (a, b) =>
                {
                    try
                    {
                        File.WriteAllText(Path.Combine(targetPath, "data", b.Index + ".json"), JsonUtils.SerializeJson(b));
                    }
                    catch
                    {
                        OnLog(sender, "ERROR -> Error writing chapter-data");
                    }
                };
                dwn.OnChapterDataFetchError += (a, b) =>
                {
                    OnLog(a, "ERROR -> " + ParseExceptionMessage(b.Exception));
                    if (errChapterInfos.FirstOrDefault(i => i.ChapterUrl == b.ChapterInfo.ChapterUrl) == null)
                        errChapterInfos.Add(b.ChapterInfo);
                };
            }

            // reset client to remove old data
            dwn.ResetClient();

            OnLog(sender, $"INFO -> Fetching - \"{e.Title}\" by \"{e.Author}\"");
            dwn.FetchNovelInfo(e.URL);
        }

        #endregion

        #region GUI Events

        private void btnCheck_Click(object sender, EventArgs e)
        {
            ResetURLWorkspace();
            LockControls();

            var novelUrl = txtURL.Text;

            // remove event handlers
            if (currentDownloader != null)
            {
                try
                {
                    currentDownloader.OnLog -= OnLog;

                    currentDownloader.OnNovelInfoFetchSuccess -= OnNovelInfoFetchSuccess;
                    currentDownloader.OnNovelInfoFetchError -= OnNovelInfoFetchError;

                    currentDownloader.OnChapterListFetchSuccess -= OnChapterListFetchSuccess;
                    currentDownloader.OnChapterListFetchError -= OnChapterListFetchError;

                    currentDownloader.OnChapterDataFetchSuccess -= OnChapterDataFetchSuccess;
                    currentDownloader.OnChapterDataFetchError -= OnChapterDataFetchError;
                }
                catch { }
            }

            currentDownloader = Downloaders.GetValidDownloader(novelUrl);

            if (currentDownloader == null)
            {
                MessageBox.Show("No matching downloaders found", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                OnLog(this, "ERROR -> Oops! No matching downloaders found.");
                return;
            }

            // add event handlers
            {
                currentDownloader.OnLog += OnLog;

                currentDownloader.OnNovelInfoFetchSuccess += OnNovelInfoFetchSuccess;
                currentDownloader.OnNovelInfoFetchError += OnNovelInfoFetchError;

                currentDownloader.OnChapterListFetchSuccess += OnChapterListFetchSuccess;
                currentDownloader.OnChapterListFetchError += OnChapterListFetchError;

                currentDownloader.OnChapterDataFetchSuccess += OnChapterDataFetchSuccess;
                currentDownloader.OnChapterDataFetchError += OnChapterDataFetchError;
            }

            // reset client to remove old data
            currentDownloader.ResetClient();

            txtConsole.AppendText("Getting Novel Information..." + Environment.NewLine);
            currentDownloader.FetchNovelInfo(novelUrl);
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnGrabChapters_Click(object sender, EventArgs e)
        {
            if (stopFetchingChapterData)
            {
                return;
            }
            else if (grabbingChapters)
            {
                btnGrabChapters.Text = "Stopping";
                stopFetchingChapterData = true;
                btnGrabChapters.Enabled = false;
                return;
            }

            grabbingChapters = true;
            ResetURLWorkspace(true);
            TargetPath = GetTargetPath(novelInfo.Title, novelInfo.Author);
            if (string.IsNullOrWhiteSpace(TargetPath))
                return;

            txtConsole.AppendText("Downloading..." + Environment.NewLine);
            LockControls();
            btnGrabChapters.Text = "STOP";
            OnLog(sender, "Fetching chapter list");
            new Task(() =>
            {
                currentDownloader.FetchChapterList();
            }).Start();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            FixImageHeight();
        }

        private void ImageLoaded(object sender, AsyncCompletedEventArgs e)
        {
            FixImageHeight();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch { }
        }

        private void btnAddToLibrary_Click(object sender, EventArgs e)
        {
            libraryUserControl1.AddToLibrary(novelInfo.NovelUrl, Path.Combine(TargetPath, "data"));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnLog(sender, "Saving library...");
            libraryUserControl1.SaveLibrary();
        }

        #endregion

        #region Helper Methods

        void ExtraInit()
        {
            txtURL.Text = "https://www.webnovel.com/book/dc's-ghost_20461293705507605";

            folderBrowserDialog1.Description = "Select the folder where novel-data will be downloaded";
            folderBrowserDialog1.ShowNewFolderButton = true;

            pictureBox1.LoadCompleted += ImageLoaded;
            libraryUserControl1.OnLog += OnLog;
            libraryUserControl1.OnStartDownloadNovelUpdate += OnStartDownloadNovelUpdate;

            txtConsole.ContextMenu = new ContextMenu((new List<MenuItem>()
            {
                new MenuItem("Clear", (a, b) =>
                {
                    txtConsole.Clear();
                })
            }).ToArray());
        }

        string ParseExceptionMessage(Exception ex)
        {
            var ex1 = ex;
            string message = ex1.Message;
            while (string.IsNullOrWhiteSpace(message))
            {
                ex1 = ex1.InnerException;
                message = ex1.Message;
            }
            return message;
        }

        void ResetURLWorkspace(bool partialReset = false)
        {
            if (!partialReset)
            {
                txtConsole.Clear();
                currentDownloader = null;
                novelInfo = null;
                TargetPath = "";
                lblTitle.Text = "NA";
                lblAuthor.Text = "NA";
                lblChapterCount.Text = "NA";
                btnGrabChapters.Enabled = false;
            }
            progDownload.Value = 0;
            btnAddToLibrary.Visible = false;
            errorChapterInfos = new List<ChapterInfo>();
        }

        void FixImageHeight()
        {
            if (pictureBox1.Image != null)
            {
                var img = pictureBox1.Image;
                var targetHeight = (int)((img.Height / (double)img.Width) * pictureBox1.Width);
                pictureBox1.Height = Math.Min(targetHeight, 200);
            }
        }

        void LockControls()
        {
            btnCheck.Enabled = false;

            txtURL.Enabled = false;

            //btnSelectFolder.Enabled = false;
            //txtFolderPath.Enabled = false;
        }

        void UnlockControls(bool enableBtnGrabChapter = false)
        {
            btnCheck.Enabled = true;
            if (enableBtnGrabChapter)
                btnGrabChapters.Enabled = true;

            txtURL.Enabled = true;

            //btnSelectFolder.Enabled = true;
            //txtFolderPath.Enabled = true;
        }

        bool IsFullPath(string path)
        {
            return !String.IsNullOrWhiteSpace(path)
                && path.IndexOfAny(System.IO.Path.GetInvalidPathChars().ToArray()) == -1
                && Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }

        string GetTargetPath(string title, string author)
        {
            var path = txtFolderPath.Text.Trim();
            try
            {
                if (!IsFullPath(path))
                    path = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), path);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var novelFolderName = author + " - " + title;
                var invalidPathChars = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToList();
                foreach (var ch in invalidPathChars)
                {
                    novelFolderName = novelFolderName.Replace(ch.ToString(), "");
                }
                path = Path.Combine(path, novelFolderName);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!Directory.Exists(Path.Combine(path, "data")))
                    Directory.CreateDirectory(Path.Combine(path, "data"));
            }
            catch
            {
                OnLog(this, "ERROR -> Invalid folder path, please select a valid filepath.");
                return "";
            }

            return path;
        }

        #endregion
    }
}
