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

namespace Novel_Downloader
{
    public partial class LibraryUserControl : UserControl
    {
        readonly string CWD;
        TextBox txtConsole = null;

        IDownloader currentDownloader = null;

        public LibraryUserControl()
        {
            InitializeComponent();
            CWD = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            tblNovelList.RowStyles.Clear();
            tblNovelList.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        }

        public void RedirectLogTo(TextBox txtConsole)
        {
            this.txtConsole = txtConsole;
        }

        public void SetLoading(bool value)
        {
            pLoading.Visible = value;
        }

        void LoadLibrary()
        {

        }


        public void RemoveHandlers()
        {
            if (currentDownloader != null)
            {
                try
                {
                    currentDownloader.OnLog -= OnLog;

                    currentDownloader.OnNovelInfoFetchSuccess -= OnNovelInfoFetchSuccess;
                    currentDownloader.OnNovelInfoFetchError -= OnNovelInfoFetchError;

                    currentDownloader.OnChapterListFetchSuccess -= OnChapterListFetchSuccess;
                    currentDownloader.OnChapterListFetchError -= OnChapterListFetchError;

                    currentDownloader.OnChapterDataFetchSuccess -= OnChapterDataFetchSuccess;
                    currentDownloader.OnChapterDataFetchError -= OnChapterDataFetchError;
                }
                catch { }
            }
        }

        public void AddHandlers()
        {
            if (currentDownloader != null)
            {
                try
                {
                    currentDownloader.OnLog += OnLog;

                    currentDownloader.OnNovelInfoFetchSuccess += OnNovelInfoFetchSuccess;
                    currentDownloader.OnNovelInfoFetchError += OnNovelInfoFetchError;

                    currentDownloader.OnChapterListFetchSuccess += OnChapterListFetchSuccess;
                    currentDownloader.OnChapterListFetchError += OnChapterListFetchError;

                    currentDownloader.OnChapterDataFetchSuccess += OnChapterDataFetchSuccess;
                    currentDownloader.OnChapterDataFetchError += OnChapterDataFetchError;
                }
                catch { }
            }
        }

        #region Downloader Events

        private void OnLog(object sender, string e)
        {
            throw new NotImplementedException();
        }

        private void OnChapterDataFetchError(object sender, ChapterDataFetchError e)
        {
            throw new NotImplementedException();
        }

        private void OnChapterDataFetchSuccess(object sender, ChapterData e)
        {
            throw new NotImplementedException();
        }

        private void OnChapterListFetchError(object sender, Exception e)
        {
            throw new NotImplementedException();
        }

        private void OnChapterListFetchSuccess(object sender, List<ChapterInfo> e)
        {
            throw new NotImplementedException();
        }

        private void OnNovelInfoFetchError(object sender, Exception e)
        {
            throw new NotImplementedException();
        }

        private void OnNovelInfoFetchSuccess(object sender, NovelInfo e)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
