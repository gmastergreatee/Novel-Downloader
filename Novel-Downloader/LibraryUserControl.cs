using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using Core.Models;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Novel_Downloader.Models.Library;

namespace Novel_Downloader
{
    public partial class LibraryUserControl : UserControl
    {
        IDownloader currentDownloader = null;
        Library novelLibrary { get; set; } = null;

        public LibraryUserControl()
        {
            InitializeComponent();

            tblNovelList.RowStyles.Clear();
            tblNovelList.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        #region Helper Methods

        private void SetLoading(bool value)
        {
            pLoading.Visible = value;
        }

        void LoadLibrary(List<LibNovelInfo> novelInfos)
        {
            var novelCount = 0;
            Invoke(new Action(() =>
            {
                SetLoading(true);
                tblNovelList.Controls.Clear();
            }));

            new Task(() =>
            {
                if (novelInfos != null && novelInfos.Count > 0)
                {
                    var rowCount = (novelInfos.Count / 2) + (novelInfos.Count % 2) - 1;
                    for (var c = 0; c < rowCount; c++)
                    {
                        Invoke(new Action(() =>
                        {
                            tblNovelList.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                        }));
                    }

                    foreach (var info in novelInfos)
                    {
                        var novelUserCtrl = new NovelUserControl(info)
                        {
                            Dock = DockStyle.Fill
                        };

                        Invoke(new Action(() =>
                        {
                            tblNovelList.Controls.Add(novelUserCtrl);
                        }));

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

        #endregion

        #region GUI Events

        private void LibraryUserControl_Load(object sender, EventArgs e)
        {
            new Task(() =>
            {
                novelLibrary = new Library();
                novelLibrary.Reload();
                LoadLibrary(novelLibrary.NovelList);
            }).Start();
        }

        private void btnAddNovel_Click(object sender, EventArgs e)
        {
            var formAddNovel = new FormAddNovel();
            formAddNovel.ShowDialog();

            if (formAddNovel.NovelInfo != null)
            {
                novelLibrary.AddNovel(formAddNovel.NovelInfo);
                LoadLibrary(novelLibrary.NovelList);
            }
        }

        #endregion
    }
}
