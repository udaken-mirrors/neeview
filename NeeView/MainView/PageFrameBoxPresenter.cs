using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using NeeView.PageFrames;

namespace NeeView.Presenter
{
    [NotifyPropertyChanged]
    public partial class PageFrameBoxPresenter : INotifyPropertyChanged, IPageFrameBox, IBookPageContext
    {
        private Config _config;
        private BookHub _bookHub;

        private IBook? _book;
        private BookContext? _bookContext;
        private PageFrameBox? _box;
        private bool _isLoading;


        public PageFrameBoxPresenter(Config config, BookHub bookHub)
        {
            _config = config;
            _bookHub = bookHub;

            _bookHub.BookChanging += BookHub_BookChanging;
            _bookHub.BookChanged += BookHub_BookChanged;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler? PageFrameBoxChanged;

        [Subscribable]
        public event EventHandler? SelectedRangeChanged;


        public bool IsEnabled => _box != null;


        public bool IsLoading => _bookHub.IsLoading || _isLoading;

        public IReadOnlyList<Page> Pages => _bookContext?.Pages ?? new List<Page>();

        public PageRange SelectedRange
        {
            get => _bookContext?.SelectedRange ?? new PageRange();
        }

        public IReadOnlyList<Page> SelectedPages
        {
            get
            {
                var bookContext = _bookContext;
                if (bookContext is null) return new List<Page>();
                return bookContext.SelectedRange.CollectPositions().Select(e => bookContext.Pages[e.Index]).Distinct().ToList();
            }
        }

        public IPageFrameBox? ValidPageFrameBox => ValidBox();


        public PageFrameBox? View => _box;


        private void BookHub_BookChanging(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                _isLoading = true;
                PageFrameBoxChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void BookHub_BookChanged(object? sender, EventArgs e)
        {
            AppDispatcher.Invoke(() =>
            {
                Open(_bookHub.GetCurrentBook());
                _isLoading = false;
                PageFrameBoxChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        private void Open(IBook? book)
        {
            Close();
            if (book is null) return;

            _book = book;

            _bookContext = new BookContext(_book, _config);
            _bookContext.PagesChanged += BookContext_PagesChanged;
            _bookContext.SelectedRangeChanged += BookContext_SelectedRangeChanged;
            _bookContext.PropertyChanged += BookContext_PropertyChanged;

            _box = new PageFrameBox(_bookContext);
            RaisePropertyChanged(nameof(View));

            RaisePropertyChanged(null);
            PagesChanged?.Invoke(this, EventArgs.Empty);
            SelectedRangeChanged?.Invoke(this, EventArgs.Empty);
        }



        private void Close()
        {
            if (_box is null) return;

            Debug.Assert(_box is PageFrameBox);
            (_box as IDisposable)?.Dispose();
            _box = null;
            RaisePropertyChanged(nameof(View));


            Debug.Assert(_bookContext is not null);
            _bookContext.PagesChanged -= BookContext_PagesChanged;
            _bookContext.SelectedRangeChanged -= BookContext_SelectedRangeChanged;
            _bookContext.PropertyChanged -= BookContext_PropertyChanged;
            _bookContext.Dispose();
            _bookContext = null;

            Debug.Assert(_book is not null);
            _book = null; // Dispose は BookHub の仕事

            GC.Collect();
            GC.WaitForFullGCComplete();
        }


        private void BookContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookContext.SelectedRange):
                    RaisePropertyChanged(nameof(SelectedRange));
                    break;
                case nameof(BookContext.Pages):
                    RaisePropertyChanged(nameof(Pages));
                    break;
            }
        }

        private void BookContext_PagesChanged(object? sender, EventArgs e)
        {
            PagesChanged?.Invoke(this, e);
        }

        private void BookContext_SelectedRangeChanged(object? sender, EventArgs e)
        {
            SelectedRangeChanged?.Invoke(this, e);
        }

        public void ReOpen()
        {
            if (_bookContext is null) return;

            _bookHub.RequestReLoad(this);
            //var memento = _bookContext.CreateMemento();
            //_bookHub.RequestLoad(memento);
        }



        #region IPageFrameBox

        private IPageFrameBox? ValidBox()
        {
            return IsLoading ? null : _box;
        }

        public DragTransformContext? CreateDragTransformContext(bool isPointContainer, bool isLoupeTransform)
        {
            return _box?.CreateDragTransformContext(isPointContainer, isLoupeTransform);
        }

        public void MoveTo(PagePosition position, LinkedListDirection direction)
        {
            ValidBox()?.MoveTo(position, direction);
        }

        public void MoveToNextPage(LinkedListDirection direction)
        {
            ValidBox()?.MoveToNextPage(direction);
        }

        public void MoveToNextFrame(LinkedListDirection direction)
        {
            ValidBox()?.MoveToNextFrame(direction);
        }

        public void ScrollToNextFrame(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin)
        {
            ValidBox()?.ScrollToNextFrame(direction, parameter, lineBreakStopMode, endMargin);
        }

        public bool ScrollToNext(LinkedListDirection direction, IScrollNTypeParameter parameter)
        {
            return ValidBox()?.ScrollToNext(direction, parameter) ?? false;
        }


        public void ResetTransform()
        {
            ValidBox()?.ResetTransform();
        }

        public void Stretch(bool ignoreViewOrigin)
        {
            ValidBox()?.Stretch(ignoreViewOrigin);
        }

        public void Reset()
        {
            if (_book is null) return;
            ReOpen();
        }

        public PageFrameTransformAccessor? CreateSelectedTransform()
        {
            return _box?.CreateSelectedTransform();
        }

        #endregion IPageFrameBox
    }
}
