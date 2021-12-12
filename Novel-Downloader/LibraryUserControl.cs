using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Core.Models;
using Novel_Downloader.Models.Library;

namespace Novel_Downloader
{
    public partial class LibraryUserControl : UserControl
    {
        IDownloader currentDownloader = null;

        public LibraryUserControl()
        {
            InitializeComponent();

            tblNovelList.RowStyles.Clear();
            tblNovelList.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        }

        private void SetLoading(bool value)
        {
            pLoading.Visible = value;
        }

        void LoadLibrary(List<LibNovelInfo> novelInfos)
        {
            var novelCount = 0;
            SetLoading(true);
            tblNovelList.Controls.Clear();
            new Task(() =>
            {
                if (novelInfos != null)
                {
                    foreach (var info in novelInfos)
                    {
                        var novelUserCtrl = new NovelUserControl();
                        novelUserCtrl.Dock = DockStyle.Fill;

                        novelCount++;
                    }
                }

                Invoke(new Action(() =>
                {
                    SetLoading(false);
                    pEmptyLibrary.Visible = novelCount <= 0;
                }));
            }).Start();
        }

        #region Downloader Events



        #endregion

    }
}
