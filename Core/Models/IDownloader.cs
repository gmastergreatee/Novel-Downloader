﻿using System;
using System.Collections.Generic;

namespace Core.Models
{
    public interface IDownloader
    {
        event EventHandler<string> OnLog;

        event EventHandler<NovelInfo> OnNovelInfoFetchSuccess;
        event EventHandler<Exception> OnNovelInfoFetchError;

        event EventHandler<List<ChapterInfo>> OnChapterListFetchSuccess;
        event EventHandler<Exception> OnChapterListFetchError;

        event EventHandler<ChapterData> OnChapterDataFetchSuccess;
        event EventHandler<ChapterDataFetchError> OnChapterDataFetchError;
        
        bool UrlMatch(string novelUrl);
        void FetchNovelInfo(string novelUrl);
        void FetchChapterList();
        void FetchChapterData(ChapterInfo chapterInfo);
        void ResetClient();
        void GenerateDocument(string targetPath);
    }
}
