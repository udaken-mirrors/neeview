using NeeView.Collections.Generic;
using NeeLaboratory.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public static class BookSourceFactory
    {
        // 本読み込み
        public static async Task<BookSource> CreateAsync(BookAddress address, BookCreateSetting setting, CancellationToken token)
        {
            // ページ生成
            var archiveEntryCollection = CreateArchiveEntryCollection(address.TargetPath.SimplePath, setting.IsRecursiveFolder, setting.ArchiveRecursiveMode, setting.IsIgnoreCache);
            var bookMemoryService = new BookMemoryService();
            var pages = await CreatePageCollection(archiveEntryCollection, setting.BookPageCollectMode, new PageContentFactory(bookMemoryService), token);

            // 再起判定は通常のディレクトリーのみ適用
            var canAutoRecursive = System.IO.Directory.Exists(address.TargetPath.SimplePath);

            // 再帰判定用サブフォルダー数カウント
            int subFolderCount = 0;
            if (canAutoRecursive && archiveEntryCollection.Mode != ArchiveEntryCollectionMode.IncludeSubArchives && !pages.Where(e => e.PageType == PageType.File).Any())
            {
                var entries = await archiveEntryCollection.GetEntriesWhereBookAsync(token);
                subFolderCount = entries.Count;
            }

            // 事前展開処理
            await PreExtractAsync(pages, token);

            // prefix設定
            SetPagePrefix(pages);

            // Validate sort mode
            var sortMode = ValidatePageSortMode(setting.SortMode, archiveEntryCollection);

            var pageCollection = new BookPageCollection(pages);
            pageCollection.InitializeSort(sortMode, token);
            var book = new BookSource(archiveEntryCollection, pageCollection, bookMemoryService);
            book.SubFolderCount = subFolderCount;

            return book;
        }

        private static PageSortMode ValidatePageSortMode(PageSortMode sortMode, ArchiveEntryCollection archiveEntryCollection)
        {
            // プレイリストならば登録順有効、それ以外は無効
            var isPlaylist = archiveEntryCollection?.Archiver is PlaylistArchive;
            var pageSortModeClass = isPlaylist ? PageSortModeClass.WithEntry : PageSortModeClass.Normal;
            return pageSortModeClass.ValidatePageSortMode(sortMode);
        }

        private static ArchiveEntryCollection CreateArchiveEntryCollection(string place, bool isRecursive, ArchiveEntryCollectionMode archiveRecursiveMode, bool isIgnoreCache)
        {
            var collectMode = isRecursive ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.CurrentDirectory;
            var collectModeIfArchive = isRecursive ? ArchiveEntryCollectionMode.IncludeSubArchives : archiveRecursiveMode;
            var collectOption = isIgnoreCache ? ArchiveEntryCollectionOption.IgnoreCache : ArchiveEntryCollectionOption.None;
            return new ArchiveEntryCollection(place, collectMode, collectModeIfArchive, collectOption);
        }

        /// <summary>
        /// ページ生成
        /// </summary>
        private static async Task<List<Page>> CreatePageCollection(ArchiveEntryCollection archiveEntryCollection, BookPageCollectMode bookPageCollectMode, PageContentFactory contentFactory, CancellationToken token)
        {
            List<ArchiveEntry> entries = bookPageCollectMode switch
            {
                BookPageCollectMode.Image => await archiveEntryCollection.GetEntriesWhereImageAsync(token),
                BookPageCollectMode.ImageAndBook => await archiveEntryCollection.GetEntriesWhereImageAndArchiveAsync(token),
                _ => await archiveEntryCollection.GetEntriesWherePageAllAsync(token),
            };
            var bookPrefix = LoosePath.TrimDirectoryEnd(archiveEntryCollection.Path);
            return entries.Select(e => CreatePage(bookPrefix, e, contentFactory, token)).ToList();
        }

        /// <summary>
        /// ページ作成
        /// </summary>
        /// <param name="entry">ファイルエントリ</param>
        /// <returns></returns>
        private static Page CreatePage(string bookPrefix, ArchiveEntry entry, PageContentFactory contentFactory, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return new Page(bookPrefix, contentFactory.CreatePageContent(entry, token));
#if false
            Page page;

            if (entry.IsImage())
            {
                if (entry.Archiver is MediaArchiver)
                {
                    page = new Page(bookPrefix, new MediaContent(entry));
                }
                else if (entry.Archiver is PdfArchiver)
                {
                    page = new Page(bookPrefix, new PdfContent(entry));
                }
                else if (Config.Current.Image.Standard.IsAnimatedGifEnabled && LoosePath.GetExtension(entry.Link ?? entry.EntryName) == ".gif")
                {
                    page = new Page(bookPrefix, new AnimatedContent(entry));
                }
                else
                {
                    page = new Page(bookPrefix, new BitmapContent(entry));
                }
            }
            else if (entry.IsBook())
            {
                page = new Page(bookPrefix, new ArchiveContent(entry));
                page.Thumbnail.IsCacheEnabled = true;
            }
            else
            {
                var type = entry.IsDirectory ? ArchiverType.FolderArchive : ArchiverManager.Current.GetSupportedType(entry.Link ?? entry.EntryName);
                switch (type)
                {
                    case ArchiverType.None:
                        if (Config.Current.Image.Standard.IsAllFileSupported)
                        {
                            entry.IsIgnoreFileExtension = true;
                            page = new Page(bookPrefix, new BitmapContent(entry));
                        }
                        else
                        {
                            page = new Page(bookPrefix, new FileContent(entry, FilePageIcon.File, null));
                        }
                        break;
                    case ArchiverType.FolderArchive:
                        page = new Page(bookPrefix, new FileContent(entry, FilePageIcon.Folder, null));
                        break;
                    default:
                        page = new Page(bookPrefix, new FileContent(entry, FilePageIcon.Archive, null));
                        break;
                }
            }

            return page;
#endif
        }


        /// <summary>
        /// PageのPrefix設定
        /// </summary>
        private static void SetPagePrefix(List<Page> pages)
        {
            // TODO: ページ生成と同時に行うべき?
            var prefix = GetPagesPrefix(pages);
            foreach (var page in pages)
            {
                page.Prefix = prefix;
            }
        }

        // 名前の最長一致文字列取得
        private static string GetPagesPrefix(List<Page> pages)
        {
            if (pages == null || pages.Count == 0) return "";

            string? s = pages[0].EntryName;
            if (s is null) return "";
            foreach (var page in pages)
            {
                s = GetStartsWith(s, page.EntryName);
                if (string.IsNullOrEmpty(s)) break;
            }

            // １ディレクトリだけの場合に表示が消えないようにする
            if (pages.Count == 1)
            {
                s = s.TrimEnd('\\', '/');
            }

            // 最初の区切り記号
            for (int i = s.Length - 1; i >= 0; --i)
            {
                if (s[i] == '\\' || s[i] == '/')
                {
                    return s[..(i + 1)];
                }
            }

            // ヘッダとして認識できなかった
            return "";
        }

        //
        private static string GetStartsWith(string s0, string s1)
        {
            if (s0 == null || s1 == null) return "";

            if (s0.Length > s1.Length)
            {
                (s1, s0) = (s0, s1);
            }

            for (int i = 0; i < s0.Length; ++i)
            {
                if (s0[i] != s1[i])
                {
                    return i > 0 ? s0[..i] : "";
                }
            }

            return s0;
        }


        // 事前展開
        // TODO: 事前展開の非同期化。ページアクセスをトリガーにする
        private static async Task PreExtractAsync(List<Page> pages, CancellationToken token)
        {
            var archivers = pages
                .Select(e => e.ArchiveEntry.Archiver)
                .Distinct()
                .WhereNotNull()
                .Where(e => !e.IsFileSystem)
                .ToList();

            foreach (var archiver in archivers)
            {
                if (await archiver.CanPreExtractAsync(token))
                {
                    Debug.WriteLine($"PreExtract: EXTRACT {archiver.EntryName}");
                    await archiver.PreExtractAsync(token);
                }
            }
        }
    }

}
