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
    public partial class Form1 : Form
    {
        #region vars

        string NovelURL { get; set; } = "";

        IEnumerable<IDownloader> downloaders = new List<IDownloader>()
        {
            new Webnovel(),
        };

        IDownloader currentDownloader = null;
        List<ChapterInfo> chapterInfos = null;
        List<ChapterInfo> errorChapterInfos = null;
        List<ChapterData> chapterDatas = null;

        #endregion

        public Form1()
        {
            InitializeComponent();
            ExtraInit();
        }

        #region Downloader Events

        // ------------------------------- Novel Information

        private void OnNovelInfoFetchSuccess(object sender, NovelInfo novelInfo)
        {
            NovelURL = novelInfo.NovelUrl;
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
                txtConsole.AppendText("Title : " + novelInfo.Title + Environment.NewLine);
                txtConsole.AppendText("Author : " + novelInfo.Author + Environment.NewLine);
                txtConsole.AppendText(novelInfo.ChapterCount + " chapters found" + Environment.NewLine);

                UnlockControls(novelInfo.ChapterCount > 0);
            }));
        }

        private void OnNovelInfoFetchError(object sender, Exception e)
        {
            Invoke(new Action(() =>
            {
                txtConsole.AppendText("Error fetching novel info." + Environment.NewLine + "---------- ERROR ----------" + Environment.NewLine + ParseExceptionMessage(e) + Environment.NewLine + "---------------------------" + Environment.NewLine);
                UnlockControls();
            }));
        }

        // ------------------------------- Chapter List

        private void OnChapterListFetchSuccess(object sender, List<ChapterInfo> e)
        {
            Invoke(new Action(() =>
            {

            }));
        }

        private void OnChapterListFetchError(object sender, Exception e)
        {
            Invoke(new Action(() =>
            {
                txtConsole.AppendText("Error fetching chapter list." + Environment.NewLine + "---------- ERROR ----------" + Environment.NewLine + ParseExceptionMessage(e) + Environment.NewLine);
                UnlockControls(true);
            }));
        }

        // ------------------------------- Chapter Data

        private void OnChapterDataFetchSuccess(object sender, ChapterData e)
        {
            Invoke(new Action(() =>
            {

            }));
        }

        private void OnChapterDataFetchError(object sender, ChapterDataFetchError e)
        {
            Invoke(new Action(() =>
            {

            }));
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

            txtConsole.AppendText("Getting Novel Information..." + Environment.NewLine);
            currentDownloader.GetNovelInfoAsync(novelUrl);
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
            LockControls();
            // ...
        }

        #endregion

        #region Helper Methods

        void ExtraInit()
        {
            foreach (var itm in downloaders)
            {
                itm.OnNovelInfoFetchSuccess += OnNovelInfoFetchSuccess;
                itm.OnNovelInfoFetchError += OnNovelInfoFetchError;

                itm.OnChapterListFetchSuccess += OnChapterListFetchSuccess;
                itm.OnChapterListFetchError += OnChapterListFetchError;

                itm.OnChapterDataFetchSuccess += OnChapterDataFetchSuccess;
                itm.OnChapterDataFetchError += OnChapterDataFetchError;
            }

            folderBrowserDialog1.Description = "Select the folder where novel-data will be downloaded";
            folderBrowserDialog1.ShowNewFolderButton = true;
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
            currentDownloader = null;
            NovelURL = "";
            lblTitle.Text = "NA";
            lblAuthor.Text = "NA";
            lblChapterCount.Text = "NA";
            btnGrabChapters.Enabled = false;
            chapterInfos = new List<ChapterInfo>();
            errorChapterInfos = new List<ChapterInfo>();
            chapterDatas = new List<ChapterData>();
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

        #endregion
    }
}
