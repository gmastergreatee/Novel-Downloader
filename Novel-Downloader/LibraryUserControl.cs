using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Text;
using Core.Models;
using System.Drawing;
using Core.Models.Library;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Novel_Downloader
{
    public partial class LibraryUserControl : UserControl
    {
        public event EventHandler<string> OnLog;
        public event EventHandler<LibNovelInfo> OnUpdate;

        Library novelLibrary { get; set; } = null;

        public LibraryUserControl()
        {
            InitializeComponent();

            tblNovelList.RowStyles.Clear();
            tblNovelList.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        #region Helper Methods

        public void AddToLibrary(LibNovelInfo novelInfo)
        {
            novelLibrary.AddNovel(novelInfo);
        }

        public void SaveLibrary()
        {
            novelLibrary.SaveNovels();
        }

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

                        novelUserCtrl.OnDeleteClick += NovelUserCtrl_OnDeleteClick;
                        novelUserCtrl.OnUpdateClick += NovelUserCtrl_OnUpdateClick;

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

                    btnUpdateInfos.Enabled = true;
                    btnAddNovel.Enabled = true;
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

        private void btnUpdateInfos_Click(object sender, EventArgs e)
        {
            btnAddNovel.Enabled = false;
            btnUpdateInfos.Enabled = false;
            new Task(() =>
            {
                try
                {
                    var ctrlCount = 0;
                    foreach (var ctrl in tblNovelList.Controls)
                    {
                        var novelCtrl = (NovelUserControl)ctrl;
                        if (!novelCtrl.IsLocked && novelCtrl.NovelInfo.CheckForUpdates)
                        {
                            novelCtrl.LockControls();

                            IDownloader downloader = Downloaders.GetValidDownloader(novelCtrl.NovelInfo.URL);
                            if (downloader == null)
                            {
                                OnLog?.Invoke(this, $@"ERROR -> ""{novelCtrl.NovelInfo.Title}"" by ""{novelCtrl.NovelInfo.Author}"" -> No matching downloader found");
                            }
                            else
                            {
                                NovelInfo info = null;

                                #region Downloader Events
                                downloader.OnLog += (a, b) =>
                                {
                                    OnLog?.Invoke(a, b);
                                };
                                downloader.OnNovelInfoFetchSuccess += (a, b) =>
                                {
                                    info = b;
                                    novelCtrl.NovelInfo.ChapterCount = info.ChapterCount;
                                    novelCtrl.UnlockControls();
                                    OnLog?.Invoke(sender, $"\"{novelCtrl.NovelInfo.Title}\" -> Info fetched");
                                    ++ctrlCount;
                                };
                                downloader.OnNovelInfoFetchError += (a, b) =>
                                {
                                    OnLog?.Invoke(a, $"ERROR ->  {b.Message}");
                                    novelCtrl.UnlockControls();
                                    ++ctrlCount;
                                };
                                #endregion

                                System.Threading.Thread.Sleep(2000);

                                OnLog?.Invoke(sender, $"\"{novelCtrl.NovelInfo.Title}\" -> Fetching info");
                                downloader.FetchNovelInfo(novelCtrl.NovelInfo.URL);
                            }
                        }
                        else
                        {
                            ++ctrlCount;
                        }
                    }

                    while (ctrlCount < tblNovelList.Controls.Count)
                        System.Threading.Thread.Sleep(1000);

                    Invoke(new Action(() =>
                    {
                        btnAddNovel.Enabled = true;
                        btnUpdateInfos.Enabled = true;
                    }));
                }
                catch { }
            }).Start();
        }

        #endregion

        #region Novel UserControl Events

        private void NovelUserCtrl_OnUpdateClick(object sender, LibNovelInfo e)
        {
            OnUpdate?.Invoke(sender, e);
        }

        private void NovelUserCtrl_OnDeleteClick(object sender, LibNovelInfo e)
        {
            novelLibrary.RemoveNovel(e);
            Invoke(new Action(() =>
            {
                LoadLibrary(novelLibrary.NovelList);
            }));
        }

        #endregion

    }
}
