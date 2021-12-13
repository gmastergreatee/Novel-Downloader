using System;
using System.IO;
using Core.Models;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Novel_Downloader.Models.Library;

namespace Novel_Downloader
{
    public partial class FormAddNovel : Form
    {
        private readonly string _cwd = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

        LibNovelInfo info;
        IDownloader currentDownloader;

        public FormAddNovel()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var url = txtURL.Text.Trim();
            currentDownloader = Downloaders.GetValidDownloader(url);

            if (currentDownloader == null)
            {
                MessageBox.Show("No applicable downloaders found. Please enter a valid URL.", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var infoJsonFile = txtDataPath.Text.Trim();
            bool infoJsonFileError;
            if (!File.Exists(infoJsonFile))
            {
                infoJsonFileError = true;
                goto infoJsonErrorLabel;
            }

            try
            {

            }
            catch
            {
                infoJsonFileError = true;
                goto infoJsonErrorLabel;
            }

            var epubPath = txtEPUBPath.Text.Trim();
            if (!epubPath.ToLower().EndsWith(".epub"))
                epubPath += ".epub";

            if (!File.Exists(epubPath))
            {
                try
                {
                    var dir = Path.GetDirectoryName(epubPath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(epubPath, "");
                }
                catch
                {
                    MessageBox.Show("Error checking EPUB file", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            info = new LibNovelInfo()
            {
                URL = url,
                DataDirPath = Path.GetDirectoryName(infoJsonFile),
                EpubFilePath = epubPath,
            };
            return;

        infoJsonErrorLabel:
            if (infoJsonFileError)
            {
                MessageBox.Show("Error checking info.json file", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
    }
}
