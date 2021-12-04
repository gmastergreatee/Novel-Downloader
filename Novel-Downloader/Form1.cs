using Novel_Downloader.Downloaders;
using Novel_Downloader.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Novel_Downloader
{
    public partial class Form1 : Form
    {
        IEnumerable<IDownloader> downloaders = new List<IDownloader>()
        {
            new Webnovel(),
        };

        IDownloader currentDownloader = null;

        void ResetURLWorkspace()
        {
            currentDownloader = null;
            lblTitle.Text = "NA";
            lblAuthor.Text = "NA";
            lblChapterCount.Text = "NA";
        }

        void LockControls()
        {
            btnCheck.Enabled = false;
            btnSelectFolder.Enabled = false;
            btnGrabChapters.Enabled = false;
            txtURL.Enabled = false;
            txtFolderPath.Enabled = false;
        }

        void UnlockControls()
        {
            btnCheck.Enabled = true;
            btnSelectFolder.Enabled = true;
            btnGrabChapters.Enabled = true;
            txtURL.Enabled = true;
            txtFolderPath.Enabled = true;
        }

        public Form1()
        {
            InitializeComponent();

            foreach (var itm in downloaders)
            {
                itm.OnNovelInfoFetchSuccess += OnNovelInfoFetchSuccess;
            }
        }

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

        private void OnNovelInfoFetchSuccess(object sender, NovelInfo novelInfo)
        {
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

                UnlockControls();
            }));
        }

        private void btnSelectFolder_Click(object sender, EventArgs e)
        {

        }

        private void btnGrabChapters_Click(object sender, EventArgs e)
        {

        }
    }
}
