using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public partial class Book : IDisposable, IBook
    {
        public static Book? Default { get; private set; }

        //private readonly BookMemoryService _bookMemoryService = new();

        private readonly BookSource _source;
        private readonly BookPageSetting _setting;
        //private readonly BookPageViewer _viewer;
        private readonly BookPageMarker _marker;
        //private readonly BookController _controller;
        private readonly string _sourcePath;
        private readonly BookLoadOption _loadOption;
        private readonly BookAddress _address;


        public Book(BookAddress address, BookSource source, BookMemento memento, BookLoadOption option, bool isNew)
        {
            Book.Default = this;

            this.Memento = memento;

            _address = address;
            _source = source;
            _sourcePath = address.SourcePath?.SimplePath ?? "";
            _setting = new BookPageSetting(CreateBookViewerCreateSetting(memento));
            //_viewer = new BookPageViewer(_source, _bookMemoryService, _setting);
            _marker = new BookPageMarker(_source);
            //_controller = new BookController(_source, _setting, _viewer, _marker);
            _loadOption = option;

            _source.Pages.PagesSorted += (s, e) => PagesChanged?.Invoke(s, e);

            IsNew = isNew;
        }


        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler? CurrentPageChanged;


        public BookSource Source => _source;
        public BookPageCollection Pages => _source.Pages;
        IReadOnlyList<Page> IBook.Pages => _source.Pages;

        private List<Page> _currentPages = new();

        public IReadOnlyList<Page> CurrentPages => _currentPages;
        public Page? CurrentPage => _currentPages.FirstOrDefault();

        public BookPageSetting Setting => _setting;
        //public BookPageViewer Viewer => _viewer;
        public BookPageMarker Marker => _marker;
        //public BookController Control => _controller;
        public BookMemoryService BookMemoryService => _source.BookMemoryService;

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


        /// <summary>
        /// 初期Memento
        /// </summary>
        public BookMemento Memento { get; private set; }


        public void SetStartPage(object? sender, BookStartPage startPage)
        {
            if (_disposedValue) return;

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
                int lastPageOffset = (_setting.PageMode == PageMode.WidePage && !_setting.IsSupportedSingleLastPage) ? 1 : 0;
                if (startPage.IsResetLastPage && index >= _source.Pages.LastPosition().Index - lastPageOffset)
                {
                    position = _source.Pages.FirstPosition();
                }
            }

            // 開始ページ記憶
            this.StartEntry = _source.Pages.Count > 0 ? _source.Pages[position.Index].EntryName : null;

            //this.Memento.Page = this.StartEntry ?? "";
            var page = _source.Pages.Count > 0 ? _source.Pages[position.Index] : null;
            if (page is not null)
            {
                SetCurrentPages(new[] { page });
            }

            // 初期ページ設定 
            //_controller.JumpPage(sender, position, direction);
        }

        public void Start()
        {
            if (_disposedValue) return;

            // TODO: スタートページへ移動
            //_controller.Start();
        }


        public void SetCurrentPages(IEnumerable<Page> pages)
        {
            if (_currentPages.SequenceEqual(pages)) return;

            _currentPages = pages.ToList();

            this.Memento.Page = CurrentPage?.EntryName ?? "";
            CurrentPageChanged?.Invoke(this, EventArgs.Empty);
        }

#if false
        public void SetCurrentPage(Page? page)
        {
            if (CurrentPage != page)
            {
                CurrentPage = page;
                this.Memento.Page = page?.EntryName ?? "";
                CurrentPageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
#endif


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

                    //_controller.Dispose();
                    //_viewer.Dispose();
                    _source.Dispose();
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

        #region Memento

        // bookの設定を取得する
        public BookMemento CreateMemento()
        {
            var memento = new BookMemento
            {
                Path = _source.Path,
                Page = _source.Pages.SortMode != PageSortMode.Random ? this.CurrentPage?.EntryName ?? "" : "",

                PageMode = _setting.PageMode,
                BookReadOrder = _setting.BookReadOrder,
                IsSupportedDividePage = _setting.IsSupportedDividePage,
                IsSupportedSingleFirstPage = _setting.IsSupportedSingleFirstPage,
                IsSupportedSingleLastPage = _setting.IsSupportedSingleLastPage,
                IsSupportedWidePage = _setting.IsSupportedWidePage,
                IsRecursiveFolder = _source.IsRecursiveFolder,
                SortMode = _source.Pages.SortMode
            };

            return memento;
        }

        // bookに設定を反映させる
        public void Restore(BookMemento memento)
        {
            if (memento == null) return;

            if (_disposedValue) return;

            _setting.PageMode = memento.PageMode;
            _setting.BookReadOrder = memento.BookReadOrder;
            _setting.IsSupportedDividePage = memento.IsSupportedDividePage;
            _setting.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            _setting.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            _setting.IsSupportedWidePage = memento.IsSupportedWidePage;
            _source.IsRecursiveFolder = memento.IsRecursiveFolder;
            _source.Pages.SortMode = memento.SortMode;
        }

        private static BookPageViewSetting CreateBookViewerCreateSetting(BookMemento memento)
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
