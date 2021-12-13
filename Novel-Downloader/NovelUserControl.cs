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
        public LibNovelInfo NovelInfo { get; private set; } = null;

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
    }
}
