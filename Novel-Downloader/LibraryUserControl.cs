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
using Core;

namespace Novel_Downloader
{
    public partial class LibraryUserControl : UserControl
    {
        public event EventHandler<string> OnLog;
        public event EventHandler<LibNovelInfo> OnStartDownloadNovelUpdate;

        List<NovelUserControl> novelUserControls = null;
        Library novelLibrary { get; set; } = null;

        public LibraryUserControl()
        {
            InitializeComponent();

            novelUserControls = new List<NovelUserControl>();
            tblNovelList.RowStyles.Clear();
            tblNovelList.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }

        #region Helper Methods

        public void AddToLibrary(string url, string dataDirPath)
        {
            var targetControl = novelUserControls.FirstOrDefault(i => i.NovelInfo.URL == url);

            var infoJsonFilePath = Path.Combine(dataDirPath, "info.json");
            if (!File.Exists(infoJsonFilePath))
            {
                OnLog?.Invoke(this, "ERROR -> info.json not found");
                return;
            }

            var nInfo = JsonUtils.DeserializeJson<NovelInfo>(File.ReadAllText(infoJsonFilePath));
            var downloadedCount = 0;
            for (var i = 0; i < nInfo.ChapterCount; i++)
            {
                if (File.Exists(Path.Combine(dataDirPath, (i + 1).ToString() + ".json")))
                    downloadedCount++;
                else
                    break;
            }

            if (targetControl == null)
            {
                var tempNovelInfo = new LibNovelInfo()
                {
                    URL = nInfo.NovelUrl,
                    DataDirPath = dataDirPath,
                    EpubFilePath = Path.Combine(Path.GetDirectoryName(dataDirPath), FileUtils.GetValidFileName(nInfo.Title, nInfo.Author) + ".epub"),
                    CheckForUpdates = true,

                    Title = nInfo.Title,
                    Author = nInfo.Author,
                    ChapterCount = nInfo.ChapterCount,
                    DownloadedTill = downloadedCount,
                    Description = "",
                };

                novelLibrary.AddNovel(tempNovelInfo);
                LoadLibrary(novelLibrary.NovelList);
                OnLog?.Invoke(this, "UPDATE -> Added to Library");
            }
            else
            {
                targetControl.NovelInfo.URL = nInfo.NovelUrl;
                targetControl.NovelInfo.DataDirPath = dataDirPath;
                targetControl.NovelInfo.EpubFilePath = Path.Combine(Path.GetDirectoryName(dataDirPath), FileUtils.GetValidFileName(nInfo.Title, nInfo.Author) + ".epub");
                targetControl.NovelInfo.Title = nInfo.Title;
                targetControl.NovelInfo.Author = nInfo.Author;
                targetControl.NovelInfo.ChapterCount = nInfo.ChapterCount;
                targetControl.NovelInfo.DownloadedTill = downloadedCount;
                targetControl.NovelInfo.Description = "";

                targetControl.UpdateComplete();
                novelLibrary.SaveNovels();
                OnLog?.Invoke(this, "UPDATE -> Updated to Library");
            }
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
            novelUserControls.Clear();

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

                        novelUserControls.Add(novelUserCtrl);

                        novelCount++;
                    }

                    Invoke(new Action(() =>
                    {
                        tblNovelList.Controls.AddRange(novelUserControls.ToArray());
                    }));
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

        public void NovelUpdated(LibNovelInfo novelInfo)
        {
            var el = novelUserControls.FirstOrDefault(i => i.NovelInfo.URL == novelInfo.URL);
            if (el != null)
            {
                el.UpdateComplete();
            }
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
                    foreach (var novelCtrl in novelUserControls)
                    {
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
                                    OnLog?.Invoke(sender, $"\"{novelCtrl.NovelInfo.Title}\" -> Info fetched");
                                    novelCtrl.UpdateComplete();
                                    ++ctrlCount;
                                };
                                downloader.OnNovelInfoFetchError += (a, b) =>
                                {
                                    OnLog?.Invoke(a, $"ERROR ->  {b.Message}");
                                    novelCtrl.UpdateComplete();
                                    ++ctrlCount;
                                };
                                #endregion

                                OnLog?.Invoke(sender, $"\"{novelCtrl.NovelInfo.Title}\" -> Updating info");
                                downloader.FetchNovelInfo(novelCtrl.NovelInfo.URL);

                                System.Threading.Thread.Sleep(2000);
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
                        OnLog?.Invoke(sender, "INFO -> All novel-info updated");
                    }));
                }
                catch { }
            }).Start();
        }

        #endregion

        #region Novel UserControl Events

        private void NovelUserCtrl_OnUpdateClick(object sender, LibNovelInfo e)
        {
            OnStartDownloadNovelUpdate?.Invoke(sender, e);
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
