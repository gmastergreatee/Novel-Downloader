using Core;
using System;
using System.IO;
using System.Linq;
using Core.Models;
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
        List<ChapterInfo> chapterInfos { get; set; } = null;
        List<ChapterInfo> errorChapterInfos { get; set; } = null;
        List<ChapterData> chapterDatas { get; set; } = null;
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

                progDownload.Value = 0;
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

            chapterInfos = e.ToList();
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
                            var thumbLoc = Path.Combine(TargetPath, "data", "image.jpg");
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
                    File.WriteAllText(Path.Combine(TargetPath, "data", "list.json"), JsonUtils.SerializeJson(chapterInfos));
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
                    chapterInfosToDownload = errorChapterInfos.ToList();
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
                chapterDatas.Add(e);

                Invoke(new Action(() =>
                {
                    progDownload.Value = chapterDatas.Count;
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
                txtConsole.AppendText("Oops! No matching downloaders found." + Environment.NewLine);
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
            SetTargetPath();
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
            btnAddToLibrary.Visible = false;
            chapterInfos = new List<ChapterInfo>();
            errorChapterInfos = new List<ChapterInfo>();
            chapterDatas = new List<ChapterData>();
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

        void SetTargetPath()
        {
            var path = txtFolderPath.Text.Trim();
            try
            {
                if (!IsFullPath(path))
                    path = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), path);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var novelFolderName = novelInfo.Author + " - " + novelInfo.Title;
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
                return;
            }

            TargetPath = path;
        }

        #endregion

    }
}
