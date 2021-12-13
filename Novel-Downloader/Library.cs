using Core;
using System.IO;
using Core.Models.Library;
using System.Collections.Generic;

namespace Novel_Downloader
{
    public class Library
    {
        public readonly List<LibNovelInfo> NovelList;

        // ReSharper disable once PossibleNullReferenceException
        private readonly string _cwd = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        private readonly string _novelListFilePath;

        public Library()
        {
            NovelList = new List<LibNovelInfo>();
            _novelListFilePath = Path.Combine(_cwd, "NovelList.nd");
        }

        public void Reload()
        {
            NovelList.Clear();
            if (File.Exists(_novelListFilePath))
            {
                var content = File.ReadAllText(_novelListFilePath);
                try
                {
                    NovelList.AddRange(JsonUtils.DeserializeJson<List<LibNovelInfo>>(content));
                    foreach (var novelInfo in NovelList)
                    {
                        var dataDirPath = novelInfo.DataDirPath;

                        var downloadedCount = 0;
                        for (var i = 0; i < novelInfo.ChapterCount; i++)
                        {
                            if (File.Exists(Path.Combine(dataDirPath, (i + 1).ToString() + ".json")))
                                downloadedCount++;
                            else
                                break;
                        }
                        novelInfo.DownloadedTill = downloadedCount;
                    }
                }
                catch
                {
                    SaveNovels();
                }
            }
            else
            {
                SaveNovels();
            }
        }

        public void AddNovel(LibNovelInfo novelInfo)
        {
            // add latest novel to the top of stack
            NovelList.Insert(0, novelInfo);
            SaveNovels();
        }

        public void RemoveNovel(LibNovelInfo novelInfo)
        {
            NovelList.Remove(novelInfo);
            SaveNovels();
        }

        public void SaveNovels()
        {
            File.WriteAllText(_novelListFilePath, JsonUtils.SerializeJson(NovelList));
        }
    }
}
