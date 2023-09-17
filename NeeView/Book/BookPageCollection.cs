using NeeLaboratory.Collection;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace NeeView
{
    public partial class BookPageCollection : BindableBase, IReadOnlyList<Page>, IEnumerable<Page>, IDisposable
    {
        // サムネイル寿命管理
        private readonly PageThumbnailPool _thumbnailPool = new();

        // ページ列
        private PageSortMode _sortMode = PageSortMode.Entry;


        public BookPageCollection(List<Page> pages)
        {
            Pages = pages;

            PageMap = Pages.ToMultiMap(e => e.EntryFullName, e => e);

            foreach (var page in Pages)
            {
                AttachPage(page);
            }

            for (int i = 0; i < Pages.Count; ++i)
            {
                Pages[i].EntryIndex = i;
            }
        }

        private void AttachPage(Page page)
        {
            page.Thumbnail.Touched += Thumbnail_Touched;
        }

        private void DetachPage(Page page)
        {
            page.Thumbnail.Touched -= Thumbnail_Touched;
        }


        // ソートされた
        [Subscribable]
        public event EventHandler? PagesSorted;

        // ファイル削除された
        [Subscribable]
        public event EventHandler<PageRemovedEventArgs>? PageRemoved;


        public List<Page> Pages { get; private set; }

        public MultiMap<string, Page> PageMap { get; private set; }

        public PageSortMode SortMode
        {
            get => _sortMode;
            set
            {
                if (SetProperty(ref _sortMode, value))
                {
                    // TODO: 直接ソート命令が呼べれば良いのだが
                    //Sort(CancellationToken.None);
                }
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.ResetPropertyChanged();
                    this.PageRemoved = null;
                    this.PagesSorted = null;

                    foreach (var page in Pages)
                    {
                        DetachPage(page);
                        page.Dispose();
                    }

                    //Pages.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region IEnumerable<Page> Support

        public IEnumerator<Page> GetEnumerator()
        {
            return Pages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region List like

        public Page this[int index]
        {
            get
            {
                try
                {
                    return Pages[index];
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            set { Pages[index] = value; }
        }

        public int Count => Pages.Count;

        public int IndexOf(Page page) => Pages.IndexOf(page);

        public Page First() => Pages.First();

        public Page Last() => Pages.Last();

        #endregion

        /// <summary>
        /// サムネイル参照イベント処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thumbnail_Touched(object? sender, EventArgs e)
        {
            if (sender is not Thumbnail thumb) return;

            _thumbnailPool.Add(thumb);
        }

        // ページ
        public Page? GetPage(int index) => Pages.Count > 0 ? Pages[ClampPageNumber(index)] : null;

        ////public Page GetPage(string name) => Pages.FirstOrDefault(e => e.EntryName == name);

        public Page? GetPageWithEntryFullName(string name)
        {
            PageMap.TryGetValue(name, out var page);
            return page;
        }


        // ページ番号
        public int GetIndex(Page page) => Pages.IndexOf(page);

        // 先頭ページの場所
        public PagePosition FirstPosition() => PagePosition.Zero;

        // 最終ページの場所
        public PagePosition LastPosition() => Pages.Count > 0 ? new PagePosition(Pages.Count - 1, 1) : FirstPosition();

        // ページ番号のクランプ
        public int ClampPageNumber(int index)
        {
            if (index > Pages.Count - 1) index = Pages.Count - 1;
            if (index < 0) index = 0;
            return index;
        }

        // ページ場所の有効判定
        public bool IsValidPosition(PagePosition position)
        {
            return (FirstPosition() <= position && position <= LastPosition());
        }

        public bool IsValidIndex(int index)
        {
            return 0 <= index && index < Pages.Count;
        }


        #region ページの並び替え

        public void InitializeSort(PageSortMode sortMode, CancellationToken token)
        {
            _sortMode = sortMode;
            Sort(token);
        }

        public void Sort(CancellationToken token)
        {
            if (_disposedValue) return;
            if (Pages.Count <= 0) return;

            var isSortFileFirst = Config.Current.Book.IsSortFileFirst;

            IEnumerable<Page>? pages = null;

            switch (SortMode)
            {
                case PageSortMode.FileName:
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e, new ComparerFileName(isSortFileFirst, token));
                    break;
                case PageSortMode.FileNameDescending:
                    pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e, new ComparerFileName(isSortFileFirst, token));
                    break;
                case PageSortMode.TimeStamp:
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.ArchiveEntry.LastWriteTime).ThenBy(e => e, new ComparerFileName(isSortFileFirst, token));
                    break;
                case PageSortMode.TimeStampDescending:
                    pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.ArchiveEntry.LastWriteTime).ThenBy(e => e, new ComparerFileName(isSortFileFirst, token));
                    break;
                case PageSortMode.Size:
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => e.ArchiveEntry.Length).ThenBy(e => e, new ComparerFileName(isSortFileFirst, token));
                    break;
                case PageSortMode.SizeDescending:
                    pages = Pages.OrderBy(e => e.PageType).ThenByDescending(e => e.ArchiveEntry.Length).ThenBy(e => e, new ComparerFileName(isSortFileFirst, token));
                    break;
                case PageSortMode.Random:
                    var random = new Random();
                    pages = Pages.OrderBy(e => e.PageType).ThenBy(e => random.Next());
                    break;
                case PageSortMode.Entry:
                    pages = Pages.OrderBy(e => e.EntryIndex);
                    break;
                case PageSortMode.EntryDescending:
                    pages = Pages.OrderByDescending(e => e.EntryIndex);
                    break;
                default:
                    throw new NotImplementedException();
            }

            try
            {
                Pages = pages.ToList();

                // ページ ナンバリング
                PagesNumbering();

                PagesSorted?.Invoke(this, EventArgs.Empty);
            }
            // NOTE: LINQ.OrderBy内でのキャンセル例外は InvalidOperationException として返される
            catch (InvalidOperationException ex) when (ex.InnerException is OperationCanceledException canceledException)
            {
                throw canceledException;
            }
        }

        /// <summary>
        /// ページ番号設定
        /// </summary>
        private void PagesNumbering()
        {
            for (int i = 0; i < Pages.Count; ++i)
            {
                Pages[i].Index = i;
            }
        }

        /// <summary>
        /// ファイル名ソート用比較クラス
        /// </summary>
        private class ComparerFileName : IComparer<Page>
        {
            private readonly int _sortFileFirstSign;
            private readonly CancellationToken _token;

            public ComparerFileName(bool isSortFileFirst, CancellationToken token)
            {
                _sortFileFirstSign = isSortFileFirst ? 1 : -1;
                _token = token;
            }

            public int Compare(Page? x, Page? y)
            {
                _token.ThrowIfCancellationRequested();

                if (x is null) return (y is null) ? 0 : -1;
                if (y is null) return 1;

                var xName = x.GetEntryFullNameTokens();
                var yName = y.GetEntryFullNameTokens();

                var limit = Math.Min(xName.Length, yName.Length);
                for (int i = 0; i < limit; ++i)
                {
                    if (xName[i] != yName[i])
                    {
                        var xIsDirectory = i + 1 != xName.Length || x.ArchiveEntry.IsDirectory;
                        var yIsDirectory = i + 1 != yName.Length || y.ArchiveEntry.IsDirectory;
                        if (xIsDirectory != yIsDirectory)
                        {
                            return (xIsDirectory ? 1 : -1) * _sortFileFirstSign;
                        }
                        return NaturalSort.Compare(xName[i], yName[i]);
                    }
                }

                return xName.Length - yName.Length;
            }
        }

        #endregion

        #region ページの削除

        // ページの削除
        // NOTE: 実際にはページ削除時にブック再読み込みを行うため、この処理は使用されない
        public void Remove(List<Page> pages)
        {
            if (_disposedValue) return;
            if (Pages.Count <= 0) return;
            if (pages == null) return;

            var removes = pages.Where(e => Pages.Contains(e)).ToList();
            if (removes.Count <= 0) return;

            foreach (var page in removes)
            {
                DetachPage(page);
                Pages.Remove(page);
                PageMap.Remove(page.EntryFullName, page);
            }

            PagesNumbering();

            PageRemoved?.Invoke(this, new PageRemovedEventArgs(removes));
        }

        // 近くの有効なページを取得
        public Page? GetValidPage(Page? page)
        {
            var index = page != null ? page.Index : 0;
            var answer = Pages.Skip(index).Concat(Pages.Take(index).Reverse()).FirstOrDefault(e => !e.IsDeleted);
            return answer;
        }

        #endregion

        #region ページリスト用現在ページ表示フラグ

        // 表示中ページ
        private List<Page> _viewPages = new();

        /// <summary>
        /// 表示中ページフラグ更新
        /// </summary>
        public void SetViewPageFlag(List<Page> viewPages)
        {
            var hidePages = _viewPages.Except(viewPages).ToList();

            foreach (var page in viewPages)
            {
                page.IsVisible = true;
            }

            foreach (var page in hidePages)
            {
                page.IsVisible = false;
            }

            _viewPages = viewPages.ToList();
        }

        #endregion

        #region フォルダーの先頭ページを取得

        public int GetNextFolderIndex(int start)
        {
            if (Pages.Count == 0 || !SortMode.IsFileNameCategory() || start < 0)
            {
                return -1;
            }

            string currentFolder = LoosePath.GetDirectoryName(Pages[start].EntryName);

            for (int index = start + 1; index < Pages.Count; ++index)
            {
                var folder = LoosePath.GetDirectoryName(Pages[index].EntryName);
                if (currentFolder != folder)
                {
                    return index;
                }
            }

            return -1;
        }

        public int GetPrevFolderIndex(int start)
        {
            if (Pages.Count == 0 || !SortMode.IsFileNameCategory() || start < 0)
            {
                return -1;
            }

            if (start == 0)
            {
                return -1;
            }

            string currentFolder = LoosePath.GetDirectoryName(Pages[start - 1].EntryName);

            for (int index = start - 1; index > 0; --index)
            {
                var folder = LoosePath.GetDirectoryName(Pages[index - 1].EntryName);
                if (currentFolder != folder)
                {
                    return index;
                }
            }

            return 0;
        }

        #endregion
    }
}
