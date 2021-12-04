using System;
using System.Collections.Generic;

namespace Novel_Downloader.Models
{
    interface IDownloader
    {
        event EventHandler<NovelInfo> OnNovelInfoFetchSuccess;
        event EventHandler<Exception> OnNovelInfoFetchError;

        event EventHandler<List<ChapterInfo>> OnChapterListFetchSuccess;
        event EventHandler<Exception> OnChapterListFetchError;

        event EventHandler<ChapterData> OnChapterDataFetchSuccess;
        event EventHandler<ChapterDataFetchError> OnChapterDataFetchError;

        bool UrlMatch(string novelUrl);
        void GetNovelInfoAsync(string novelUrl);
        IEnumerable<ChapterInfo> GetChapterList(string novelUrl);
        ChapterData GetChapterData(string chapterUrl);
        void ResetClient();
    }
}
