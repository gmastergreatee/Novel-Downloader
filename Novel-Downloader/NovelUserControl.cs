using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Novel_Downloader.Models.Library;
using System.IO;

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

        private void NovelUserControl_Load(object sender, EventArgs e)
        {
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
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            LockControls();
            if (MessageBox.Show($@"Do you really want to delete the novel ""{NovelInfo.Title}"" by ""{NovelInfo.Author}"" from the library ?", "Really?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                OnDeleteClick?.Invoke(this, NovelInfo);
            }
            UnlockControls();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            LockControls();
            OnUpdateClick?.Invoke(this, NovelInfo);
            UnlockControls();
        }

        public void LockControls()
        {
            IsLocked = true;
            btnDelete.Enabled = false;
            btnUpdate.Enabled = false;
        }

        public void UnlockControls()
        {
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
            IsLocked = false;
        }
    }
}
