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

namespace Novel_Downloader
{
    public partial class LibraryUserControl : UserControl
    {
        readonly string CWD;
        public event EventHandler<string> OnLog;

        public LibraryUserControl()
        {
            InitializeComponent();
            CWD = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            tblNovelList.RowStyles.Clear();
            tblNovelList.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        }

        public void SetLoading(bool value)
        {
            pLoading.Visible = value;
        }

        void LoadLibrary()
        {

        }
    }
}
