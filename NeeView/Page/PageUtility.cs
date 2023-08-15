﻿using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Effects;

namespace NeeView
{
    /// <summary>
    /// ページユーティリティ
    /// </summary>
    public static class PageUtility
    {
        /// <summary>
        /// ページ群の実ファイルリストに変換可能か
        /// </summary>
        public static bool CanCreateRealizedFilePathList(IEnumerable<Page> pages)
        {
            return pages.All(e => e.Entry.IsFileSystem || !e.Content.Entry.IsArchiveDirectory());
        }

        /// <summary>
        /// ページ群の実ファイルリストを取得
        /// </summary>
        public static List<string> CreateRealizedFilePathList(IEnumerable<Page> pages, CancellationToken token)
        {
            return CreateFilePathList(pages, MultiPagePolicy.All, ArchivePolicy.SendExtractFile, token);
        }


        public static List<string> CreateFilePathList(IEnumerable<Page> pages, MultiPagePolicy multiPagePolicy, ArchivePolicy archivePolicy, CancellationToken token)
        {
            var files = new List<string>();

            foreach (var page in pages)
            {
                token.ThrowIfCancellationRequested();

                // file
                if (page.Entry.IsFileSystem)
                {
                    // TODO: IsFileSystemのときはGetFilePlace()はnullでないはず
                    var path = page.GetFilePlace();
                    if (path is not null)
                    {
                        files.Add(path);
                    }
                }
                else if (page.Entry.Instance is ArchiveEntry archiveEntry && archiveEntry.IsFileSystem)
                {
                    files.Add(archiveEntry.EntryFullName);
                }
                // in archive
                else
                {
                    switch (archivePolicy)
                    {
                        case ArchivePolicy.None:
                            break;

                        case ArchivePolicy.SendArchiveFile:
                            var path = page.GetFilePlace();
                            if (path is not null)
                            {
                                files.Add(path);
                            }
                            break;

                        case ArchivePolicy.SendExtractFile:
                            if (!page.Content.Entry.IsArchiveDirectory())
                            {
                                files.Add(page.Content.CreateTempFile(true).Path);
                            }
                            else
                            {
                                Debug.WriteLine($"CreateFilePathList: Not support archive folder: {page.EntryName}");
                                files.Add(page.Entry.EntryFullName);
                            }
                            break;

                        case ArchivePolicy.SendArchivePath:
                            files.Add(page.Entry.EntryFullName);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(archivePolicy));
                    }
                }
                if (multiPagePolicy == MultiPagePolicy.Once) break;
            }

            return files.Distinct().ToList();
        }
    }
}
