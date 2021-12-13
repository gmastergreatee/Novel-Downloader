using Core;
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

        public LibNovelInfo NovelInfo { get; private set; } = null;

        public FormAddNovel()
        {
            InitializeComponent();
            openFileDialog1.InitialDirectory = _cwd;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var infoJsonFilePath = txtDataPath.Text.Trim();
            if (!File.Exists(infoJsonFilePath))
            {
                MessageBox.Show("Error checking info.json file", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var novelInfo = JsonUtils.DeserializeJson<NovelInfo>(File.ReadAllText(infoJsonFilePath));
                var dataDirPath = Path.GetDirectoryName(infoJsonFilePath);

                var downloadedCount = 0;
                for (var i = 0; i < novelInfo.ChapterCount; i++)
                {
                    if (File.Exists(Path.Combine(dataDirPath, (i + 1).ToString() + ".json")))
                        downloadedCount++;
                    else
                        break;
                }

                NovelInfo = new LibNovelInfo()
                {
                    URL = novelInfo.NovelUrl,
                    DataDirPath = dataDirPath,
                    EpubFilePath = Path.Combine(dataDirPath, FileUtils.GetValidFileName(novelInfo.Title, novelInfo.Author) + ".epub"),

                    Title = novelInfo.Title,
                    Author = novelInfo.Author,
                    ChapterCount = novelInfo.ChapterCount,
                    DownloadedTill = downloadedCount,
                    Description = "",
                };
            }
            catch
            {
                MessageBox.Show("Error checking info.json file, maybe CORRUPT", "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Close();
        }

        private void btnSelectInfoJSONPath_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Title = "Select \"info.json\" file";
            openFileDialog1.Filter = "info.json File|info.json";


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                txtDataPath.Text = openFileDialog1.FileName;
            else
                txtDataPath.Text = "";
        }
    }
}
