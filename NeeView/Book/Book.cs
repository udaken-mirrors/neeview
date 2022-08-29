using NeeView.Collections.Generic;
using System;
using System.Diagnostics;

namespace NeeView
{
    public partial class Book : IDisposable
    {
        public static Book? Default { get; private set; }

        private BookMemoryService _bookMemoryService = new BookMemoryService();

        private BookSource _source;
        private BookPageViewer _viewer;
        private BookPageMarker _marker;
        private BookController _controller;
        private string _sourcePath;
        private BookLoadOption _loadOption;
        private BookAddress _address;


        public Book(BookAddress address, BookSource source, Book.Memento memento, BookLoadOption option, bool isNew)
        {
            Book.Default = this;

            _address = address;
            _source = source;
            _sourcePath = address.SourcePath?.SimplePath ?? "";
            _viewer = new BookPageViewer(_source, _bookMemoryService, CreateBookViewerCreateSetting(memento));
            _marker = new BookPageMarker(_source, _viewer);
            _controller = new BookController(_source, _viewer, _marker);
            _loadOption = option;

            IsNew = isNew;
        }


        public BookSource Source => _source;
        public BookPageCollection Pages => _source.Pages;
        public BookPageViewer Viewer => _viewer;
        public BookPageMarker Marker => _marker;
        public BookController Control => _controller;
        public BookMemoryService BookMemoryService => _bookMemoryService;

        public BookAddress BookAddress => _address;
        public string Path => _source.Path;
        public string SourcePath => _sourcePath;
        public bool IsMedia => _source.IsMedia;
        public bool IsPlaylist => _source.IsPlaylist;
        public bool IsTemporary => _source.Path.StartsWith(Temporary.Current.TempDirectory);

        public PageSortModeClass PageSortModeClass => IsPlaylist ? PageSortModeClass.WithEntry : PageSortModeClass.Normal;
        public BookLoadOption LoadOption => _loadOption;

        // はじめて開く本
        public bool IsNew { get; private set; }

        // 見つからなかった開始ページ名。通知用。
        // TODO: 不要？
        public string? NotFoundStartPage { get; private set; }

        // 開始ページ
        // TODO: 再読込時に必要だが、なくすことできそう？
        public string? StartEntry { get; private set; }

        public bool IsKeepHistoryOrder => (_loadOption & BookLoadOption.KeepHistoryOrder) == BookLoadOption.KeepHistoryOrder;



        public void SetStartPage(object? sender, BookStartPage startPage)
        {
            // スタートページ取得
            PagePosition position = _source.Pages.FirstPosition();
            int direction = 1;
            if (startPage.StartPageType == BookStartPageType.FirstPage)
            {
                position = _source.Pages.FirstPosition();
                direction = 1;
            }
            else if (startPage.StartPageType == BookStartPageType.LastPage)
            {
                position = _source.Pages.LastPosition();
                direction = -1;
            }
            else
            {
                int index = !string.IsNullOrEmpty(startPage.PageName) ? _source.Pages.FindIndex(e => e.EntryName == startPage.PageName) : 0;
                if (index < 0)
                {
                    this.NotFoundStartPage = startPage.PageName;
                }
                position = index >= 0 ? new PagePosition(index, 0) : _source.Pages.FirstPosition();
                direction = 1;

                // 最終ページリセット
                // NOTE: ワイドページ判定は行わないため、2ページモードの場合に不正確な場合がある
                int lastPageOffset = (_viewer.PageMode == PageMode.WidePage && !_viewer.IsSupportedSingleLastPage) ? 1 : 0;
                if (startPage.IsResetLastPage && index >= _source.Pages.LastPosition().Index - lastPageOffset)
                {
                    position = _source.Pages.FirstPosition();
                }
            }

            // 開始ページ記憶
            this.StartEntry = _source.Pages.Count > 0 ? _source.Pages[position.Index].EntryName : null;

            // 初期ページ設定 
            _controller.RequestSetPosition(sender, position, direction);
        }

        public void Start()
        {
            // TODO: スタートページへ移動
            _controller.Start();
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Book.Default == this)
                    {
                        Book.Default = null;
                    }

                    _controller.Dispose();
                    _viewer.Dispose();
                    _source.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Memento

        // bookの設定を取得する
        public Book.Memento CreateMemento()
        {
            var memento = new Book.Memento();

            memento.Path = _source.Path;
            memento.IsDirectorty = _source.IsDirectory;
            memento.Page = _source.Pages.SortMode != PageSortMode.Random ? _viewer.GetViewPage()?.EntryName : null;

            memento.PageMode = _viewer.PageMode;
            memento.BookReadOrder = _viewer.BookReadOrder;
            memento.IsSupportedDividePage = _viewer.IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = _viewer.IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = _viewer.IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = _viewer.IsSupportedWidePage;
            memento.IsRecursiveFolder = _source.IsRecursiveFolder;
            memento.SortMode = _source.Pages.SortMode;

            return memento;
        }

        // bookに設定を反映させる
        public void Restore(Book.Memento memento)
        {
            if (memento == null) return;

            if (_disposedValue) return;

            _viewer.PageMode = memento.PageMode;
            _viewer.BookReadOrder = memento.BookReadOrder;
            _viewer.IsSupportedDividePage = memento.IsSupportedDividePage;
            _viewer.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            _viewer.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            _viewer.IsSupportedWidePage = memento.IsSupportedWidePage;
            _source.IsRecursiveFolder = memento.IsRecursiveFolder;
            _source.Pages.SortMode = memento.SortMode;
        }

        private static BookPageViewSetting CreateBookViewerCreateSetting(Book.Memento memento)
        {
            var setting = new BookPageViewSetting
            {
                PageMode = memento.PageMode,
                BookReadOrder = memento.BookReadOrder,
                IsSupportedDividePage = memento.IsSupportedDividePage,
                IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage,
                IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage,
                IsSupportedWidePage = memento.IsSupportedWidePage
            };
            return setting;
        }

        #endregion
    }

}
