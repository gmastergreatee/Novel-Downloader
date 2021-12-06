﻿using Newtonsoft.Json;
using Novel_Downloader.Downloaders;
using Novel_Downloader.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Novel_Downloader
{
    public partial class MainForm : Form
    {
        #region vars

        NovelInfo novelInfo { get; set; } = null;

        IEnumerable<IDownloader> downloaders = new List<IDownloader>()
        {
            new Webnovel(),
        };

        IDownloader currentDownloader { get; set; } = null;
        List<ChapterInfo> chapterInfos { get; set; } = null;
        List<ChapterInfo> errorChapterInfos { get; set; } = null;
        List<ChapterData> chapterDatas { get; set; } = null;
        bool stopFetchingChapterData { get; set; } = false;
        string TargetPath { get; set; } = "";

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
                if (!string.IsNullOrWhiteSpace(novelInfo.ImageUrl))
                    pictureBox1.ImageLocation = novelInfo.ImageUrl;

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
            errorChapterInfos = e.ToList();
            new Task(() =>
            {
                OnLog(this, "Saving novel-info...");
                try
                {
                    File.WriteAllText(Path.Combine(TargetPath, "info.json"), JsonConvert.SerializeObject(novelInfo));
                }
                catch
                {
                    OnLog(sender, "ERROR -> Error writing novel-info");
                    return;
                }

                OnLog(this, "Saving chapter-list...");
                try
                {
                    File.WriteAllText(Path.Combine(TargetPath, "list.json"), JsonConvert.SerializeObject(chapterInfos));
                }
                catch
                {
                    OnLog(sender, "ERROR -> Error writing chapter-list");
                    return;
                }

                var count = 1;
                while (errorChapterInfos.Count > 0)
                {
                    foreach (var chapter in errorChapterInfos)
                    {
                        OnLog(
                            this,
                            "[" + count + "/" + chapterInfos.Count + "] Downloading -> " +
                            (string.IsNullOrWhiteSpace(chapter.ChapterName) ? "..." : chapter.ChapterName)
                        );
                        currentDownloader.FetchChapterData(chapter);

                        chapterInfos.Add(chapter);
                        errorChapterInfos.Remove(chapter);

                        if (stopFetchingChapterData)
                            break;

                        count++;
                    }

                    if (stopFetchingChapterData)
                        break;

                    var forceBreak = false;
                    if (errorChapterInfos.Count > 0)
                    {
                        Invoke(new Action(() =>
                        {
                            if (MessageBox.Show(errorChapterInfos.Count + " chapters weren't downloaded. Do you want to retry ?", "Query", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
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
                    stopFetchingChapterData = false;
                    OnLog(sender, "Download stopped");
                }

                OnLog(sender, count + " chapters fetched");

                OnLog(sender, "{DEVELOPER_PROMPT} -> CODE TO GENERATE EPUB HERE");
            }).Start();
        }

        private void OnChapterListFetchError(object sender, Exception e)
        {
            OnLog(sender, "ERROR -> Error fetching chapter list." + Environment.NewLine + "---------- ERROR ----------" + Environment.NewLine + ParseExceptionMessage(e) + Environment.NewLine);

            Invoke(new Action(() =>
            {
                UnlockControls(true);
            }));
        }

        // ------------------------------- Chapter Data

        private void OnChapterDataFetchSuccess(object sender, ChapterData e)
        {
            try
            {
                File.WriteAllText(Path.Combine(TargetPath, e.Index + ".json"), JsonConvert.SerializeObject(e));
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

        #region UI Events

        private void btnCheck_Click(object sender, EventArgs e)
        {
            ResetURLWorkspace();
            LockControls();

            var novelUrl = txtURL.Text;

            foreach (var itm in downloaders)
            {
                if (itm.UrlMatch(novelUrl))
                {
                    currentDownloader = itm;
                    break;
                }
            }

            if (currentDownloader == null)
            {
                MessageBox.Show("No matching downloaders found", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtConsole.AppendText("Oops! No matching downloaders found." + Environment.NewLine);
                return;
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
            var path = txtFolderPath.Text.Trim();
            try
            {
                if (!IsFullPath(path))
                    path = Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), path);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                var novelFolderName = novelInfo.Title;
                var invalidPathChars = Path.GetInvalidFileNameChars();
                foreach (var ch in invalidPathChars)
                {
                    novelFolderName.Replace(ch.ToString(), "");
                }
                path = Path.Combine(path, novelFolderName);
            }
            catch
            {
                txtConsole.AppendText("ERROR -> Invalid folder path, please select a valid filepath.");
                return;
            }

            TargetPath = path;

            txtConsole.AppendText("Downloading..." + Environment.NewLine);
            LockControls();
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

        #endregion

        #region Helper Methods

        void ExtraInit()
        {
            foreach (var itm in downloaders)
            {
                itm.OnLog += OnLog;

                itm.OnNovelInfoFetchSuccess += OnNovelInfoFetchSuccess;
                itm.OnNovelInfoFetchError += OnNovelInfoFetchError;

                itm.OnChapterListFetchSuccess += OnChapterListFetchSuccess;
                itm.OnChapterListFetchError += OnChapterListFetchError;

                itm.OnChapterDataFetchSuccess += OnChapterDataFetchSuccess;
                itm.OnChapterDataFetchError += OnChapterDataFetchError;
            }

            folderBrowserDialog1.Description = "Select the folder where novel-data will be downloaded";
            folderBrowserDialog1.ShowNewFolderButton = true;

            pictureBox1.LoadCompleted += ImageLoaded;
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

        void ResetURLWorkspace()
        {
            txtConsole.Clear();
            currentDownloader = null;
            novelInfo = null;
            TargetPath = "";
            lblTitle.Text = "NA";
            lblAuthor.Text = "NA";
            lblChapterCount.Text = "NA";
            btnGrabChapters.Enabled = false;
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
            btnSelectFolder.Enabled = false;
            btnGrabChapters.Enabled = false;

            txtURL.Enabled = false;
            txtFolderPath.Enabled = false;
        }

        void UnlockControls(bool enableBtnGrabChapter = false)
        {
            btnCheck.Enabled = true;
            btnSelectFolder.Enabled = true;
            if (enableBtnGrabChapter)
                btnGrabChapters.Enabled = true;

            txtURL.Enabled = true;
            txtFolderPath.Enabled = true;
        }

        bool IsFullPath(string path)
        {
            return !String.IsNullOrWhiteSpace(path)
                && path.IndexOfAny(System.IO.Path.GetInvalidPathChars().ToArray()) == -1
                && Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }

        #endregion

    }
}
