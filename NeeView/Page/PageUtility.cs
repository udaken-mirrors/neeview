using System.Collections.Generic;
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
            return pages.Any() && pages.All(e => e.ArchiveEntry.CanRealize());
        }

        /// <summary>
        /// ページ群の実ファイルリストを取得
        /// </summary>
        public static async Task<List<string>> CreateRealizedFilePathListAsync(IEnumerable<Page> pages, CancellationToken token)
        {
            return await CreateFilePathListAsync(pages, ArchivePolicy.SendExtractFile, token);
        }

        public static async Task<List<string>> CreateFilePathListAsync(IEnumerable<Page> pages, ArchivePolicy archivePolicy, CancellationToken token)
        {
            var files = new List<string>();
            foreach (var page in pages)
            {
                var path = await page.ArchiveEntry.RealizeAsync(archivePolicy, token);
                if (path != null)
                {
                    files.Add(path);
                }
            }
            return files.Distinct().ToList();
        }
    }
}
