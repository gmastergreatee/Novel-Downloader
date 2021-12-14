using Core;
using System;
using System.IO;
using Core.Models;
using Core.Models.Library;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Novel_Downloader
{
    public partial class NovelUserControl : UserControl
    {
        public event EventHandler<LibNovelInfo> OnDeleteClick;
        public event EventHandler<LibNovelInfo> OnUpdateClick;

        public LibNovelInfo NovelInfo { get; private set; } = null;
        public bool IsLocked { get; set; } = false;

        public NovelUserControl(LibNovelInfo novelInfo)
        {
            NovelInfo = novelInfo;
            InitializeComponent();
        }

        #region GUI Events

        private void NovelUserControl_Load(object sender, EventArgs e)
        {
            if (!NovelInfo.CheckForUpdates)
            {
                chkUpdates.Checked = false;
            }

            lblTitle.Text = NovelInfo.Title;
            lblAuthor.Text = NovelInfo.Author;
            lblChapterCount.Text = NovelInfo.ChapterCount.ToString();
            lblDownloadedChapterCount.Text = NovelInfo.DownloadedTill.ToString();
            toolTip1.SetToolTip(panel1, NovelInfo.Description);

            if (File.Exists(Path.Combine(NovelInfo.DataDirPath, "thumb.jpg")))
            {
                picNovelImage.ImageLocation = Path.Combine(NovelInfo.DataDirPath, "thumb.jpg");
            }
            else if (File.Exists(Path.Combine(NovelInfo.DataDirPath, "image.jpg")))
            {
                picNovelImage.ImageLocation = Path.Combine(NovelInfo.DataDirPath, "image.jpg");
            }

            if (NovelInfo.DownloadedTill != NovelInfo.ChapterCount && NovelInfo.ChapterCount > NovelInfo.DownloadedTill)
            {
                lblUpdateText.Text = (NovelInfo.ChapterCount - NovelInfo.DownloadedTill).ToString() + " chapters available";
                btnUpdate.Visible = true;
            }
            else
            {
                lblUpdateText.Text = "";
                btnUpdate.Visible = false;
            }

            new Task(() =>
            {
                try
                {
                    var notDownloaded = 0;
                    var list = JsonUtils.DeserializeJson<List<ChapterInfo>>(File.ReadAllText(Path.Combine(NovelInfo.DataDirPath, "list.json")));

                    foreach (var chap in list)
                    {
                        if (!File.Exists(Path.Combine(NovelInfo.DataDirPath, $"{chap.Index}.json")))
                            notDownloaded++;
                    }

                    if (notDownloaded > 0)
                    {
                        NovelInfo.DownloadedTill = NovelInfo.ChapterCount - notDownloaded;

                        Invoke(new Action(() =>
                        {
                            lblDownloadedChapterCount.Text = NovelInfo.DownloadedTill.ToString();

                            if (NovelInfo.DownloadedTill != NovelInfo.ChapterCount && NovelInfo.ChapterCount > NovelInfo.DownloadedTill)
                            {
                                lblUpdateText.Text = (NovelInfo.ChapterCount - NovelInfo.DownloadedTill).ToString() + " chapters available";
                                btnUpdate.Visible = true;
                            }
                            else
                            {
                                lblUpdateText.Text = "";
                                btnUpdate.Visible = false;
                            }
                        }));
                    }
                }
                catch { }
            }).Start();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            LockControls();
            if (MessageBox.Show($"Do you really want to delete the novel \n\n\"{NovelInfo.Title}\"\nby\n\"{NovelInfo.Author}\"\n\nfrom the library ?", "Really?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                OnDeleteClick?.Invoke(this, NovelInfo);
            }
            UnlockControls();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            LockControls();
            OnUpdateClick?.Invoke(this, NovelInfo);
        }

        private void chkUpdates_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUpdates.Checked != NovelInfo.CheckForUpdates)
            {
                NovelInfo.CheckForUpdates = chkUpdates.Checked;
            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Path.GetDirectoryName(NovelInfo.EpubFilePath));
        }

        #endregion

        #region Helper Methods

        public void UpdateComplete()
        {
            Invoke(new Action(() =>
            {
                NovelUserControl_Load(this, null);
                UnlockControls();
            }));
        }

        public void LockControls()
        {
            Invoke(new Action(() =>
            {
                panel1.BackColor = System.Drawing.SystemColors.InactiveCaption;
                IsLocked = true;
                btnDelete.Enabled = false;
                btnUpdate.Enabled = false;
            }));
        }

        public void UnlockControls()
        {
            Invoke(new Action(() =>
            {
                panel1.BackColor = System.Drawing.Color.Transparent;
                btnUpdate.Enabled = true;
                btnDelete.Enabled = true;
                IsLocked = false;
            }));
        }

        public void HideMe()
        {
            Visible = false;
        }

        public void ShowMe()
        {
            Visible = true;
        }

        #endregion

    }
}
