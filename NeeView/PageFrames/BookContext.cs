using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 現在ブック情報
    /// </summary>
    [NotifyPropertyChanged]
    public partial class BookContext : INotifyPropertyChanged, IDisposable, IBookPageContext
    {
        private readonly Book _book;
        private readonly BookPageAccessor _pageAccessor;
        private PageRange _selectedRange;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        public BookContext(Book book)
        {
            _book = book;
            _disposables.Add(_book.SubscribePagesChanged(Book_PagesChanged));

            _disposables.Add(_book.Pages.SubscribePropertyChanged(nameof(BookPageCollection.SortMode),
                (s, e) => RaisePropertyChanged(nameof(SortMode))));

            _pageAccessor = new BookPageAccessor(_book.Pages);
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;
        
        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler? SelectedRangeChanged;


        public Book Book => _book;

        public bool IsEnabled => _book.Pages.Any();

        public bool IsMedia => _book.IsMedia;

        public IReadOnlyList<Page> Pages => _book.Pages;

        public BookPageAccessor PageAccessor => _pageAccessor;

        public PageSortMode SortMode => _book.Pages.SortMode;


        public PageRange SelectedRange
        {
            get { return _selectedRange; }
            set
            {
                if (SetProperty(ref _selectedRange, value))
                {
                    _book.SetCurrentPages(SelectedPages);
                    SelectedRangeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public IReadOnlyList<Page> SelectedPages
        {
            get => _selectedRange.CollectPositions().Select(e => Pages[e.Index]).Distinct().ToList();
        }

        // NOTE: これは Book で保持する必要ある？ -> Page生成時に必要なのだ...
        public BookMemoryService BookMemoryService => _book.BookMemoryService;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Book_PagesChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Pages));
            PagesChanged?.Invoke(this, e);
        }

#if false
        public BookMemento CreateMemento()
        {
            var bookSetting = _config.BookSetting;

            var memento = new BookMemento
            {
                Path = _book.Path,
                Page = this.Pages[this.SelectedRange.Min.Index].EntryName,

                PageMode = bookSetting.PageMode,
                BookReadOrder = bookSetting.BookReadOrder,
                IsSupportedDividePage = bookSetting.IsSupportedDividePage,
                IsSupportedSingleFirstPage = bookSetting.IsSupportedSingleFirstPage,
                IsSupportedSingleLastPage = bookSetting.IsSupportedSingleLastPage,
                IsSupportedWidePage = bookSetting.IsSupportedWidePage,
                IsRecursiveFolder = bookSetting.IsRecursiveFolder,
                SortMode = bookSetting.SortMode,
            };

            return memento;
        }
#endif
    }
}
