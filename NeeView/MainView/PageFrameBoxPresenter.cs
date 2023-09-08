using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.Threading.Tasks;
using NeeView.ComponentModel;
using NeeView.PageFrames;
using NeeView.Windows;

namespace NeeView
{
    public class PageFrameBoxChangingEventArgs : BookChangingEventArgs
    {
        public PageFrameBoxChangingEventArgs(PageFrameBox? box, BookChangingEventArgs e)
            : base(e.Address)
        {
            Box = box;
        }

        public PageFrameBox? Box { get; }
    }

    public class PageFrameBoxChangedEventArgs : BookChangedEventArgs
    {
        public PageFrameBoxChangedEventArgs(PageFrameBox? box, BookChangedEventArgs e)
            : base(e.Address, e.Book, e.BookMementoType)
        {
            Box = box;
        }

        public PageFrameBox? Box { get; }
    }


    [NotifyPropertyChanged]
    public partial class PageFrameBoxPresenter : INotifyPropertyChanged, IDragTransformContextFactory, IBookPageContext
    {
        public static PageFrameBoxPresenter Current { get; } = new PageFrameBoxPresenter();

        private readonly Config _config;
        private readonly BookHub _bookHub;

        private Book? _book;
        private BookContext? _bookContext;
        private PageFrameContext? _context;
        private readonly BookShareContext _shareContext;
        private PageFrameBox? _box;
        private BookCommandControl? _pageControl;
        private BookMementoControl? _bookMementoControl;
        private bool _isLoading;


        private PageFrameBoxPresenter()
        {
            _config = Config.Current;
            _bookHub = BookHub.Current;

            _shareContext = new BookShareContext(_config);

            _bookHub.BookChanging += BookHub_BookChanging;
            _bookHub.BookChanged += BookHub_BookChanged;
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler<PageFrameBoxChangingEventArgs>? PageFrameBoxChanging;

        [Subscribable]
        public event EventHandler<PageFrameBoxChangedEventArgs>? PageFrameBoxChanged;

        [Subscribable]
        public event EventHandler? SelectedRangeChanged;

        [Subscribable]
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;

        [Subscribable]
        public event EventHandler? SelectedContainerLayoutChanged;

        [Subscribable]
        public event EventHandler? SelectedContentSizeChanged;

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;

        [Subscribable]
        public event SizeChangedEventHandler? ViewSizeChanged;

        // ロード中通知
        [Subscribable]
        public event EventHandler<BookPathEventArgs>? Loading;

        [Subscribable]
        public event EventHandler<ViewPageChangedEventArgs>? ViewPageChanged;


        public bool IsEnabled => _box != null;

        public bool IsLoading => _isLoading;

        public IReadOnlyList<Page> Pages => _box?.Pages ?? new List<Page>();

        public PageRange SelectedRange
        {
            get => _box?.SelectedRange ?? new PageRange();
        }

        public IReadOnlyList<Page> SelectedPages
        {
            get
            {
                var box = _box;
                if (box is null) return new List<Page>();
                return box.SelectedRange.CollectPositions().Select(e => box.Pages[e.Index]).Distinct().ToList();
            }
        }

        public IPageFrameBox? ValidPageFrameBox => ValidBox();


        public PageFrameBox? View => _box;
        public double ViewWidth => _box?.ActualWidth ?? 0.0;
        public double ViewHeight => _box?.ActualHeight ?? 0.0;

        public BookCommandControl? PageControl => _pageControl;

        public bool IsMedia => _box?.Book.IsMedia ?? false;



        private void BookHub_BookChanging(object? sender, BookChangingEventArgs e)
        {
            _openCancellationTokenSource.Cancel();
            _openCancellationTokenSource.Dispose();
            _openCancellationTokenSource = new();

            AppDispatcher.Invoke(() =>
            {
                SetLoading(e.Address);
                PageFrameBoxChanging?.Invoke(this, new PageFrameBoxChangingEventArgs(null, e));
            });
        }

        private CancellationTokenSource _openCancellationTokenSource = new();

        private void BookHub_BookChanged(object? sender, BookChangedEventArgs e)
        {
            AppDispatcher.BeginInvoke(async () =>
            {
                try
                {
                    await OpenAsync(_bookHub.GetCurrentBook(), _openCancellationTokenSource.Token);
                    _isLoading = false; // NOTE: RaiseViewPageChanged() で _isLoading を参照しているため先にフラグリセットしておく
                    PageFrameBoxChanged?.Invoke(this, new PageFrameBoxChangedEventArgs(_box, e));
                    RaiseViewPageChanged();
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    SetLoading(null);
                }

                MemoryControl.Current.GarbageCollect();
            });
        }

        private void SetLoading(string? address)
        {
            _isLoading = !string.IsNullOrEmpty(address);
            RaisePropertyChanged(nameof(IsLoading));
            Loading?.Invoke(this, new BookPathEventArgs(address));
        }

        private async Task OpenAsync(Book? book, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            Close();
            if (book is null)
            {
                RaisePropertyChanged(nameof(View));
                RaisePropertyChanged(null);
                PagesChanged?.Invoke(this, EventArgs.Empty);
                SelectedRangeChanged?.Invoke(this, EventArgs.Empty);

                ViewContentChanged?.Invoke(this, new FrameViewContentChangedEventArgs(ViewContentChangedAction.ContentLoaded, Array.Empty<ViewContent>(), 1));
                RaiseViewPageChanged(new ViewPageChangedEventArgs(Array.Empty<Page>()));

                return;
            }

            _book = book;

            _context = new PageFrameContext(_config, _shareContext);
            _bookContext = new BookContext(_book);

            _box = new PageFrameBox(_context, _bookContext);
            _box.PagesChanged += Box_PagesChanged;
            _box.SelectedRangeChanged += Box_SelectedRangeChanged;
            _box.PropertyChanged += Box_PropertyChanged;
            _box.ViewContentChanged += Box_ViewContentChanged;
            _box.TransformChanged += Box_TransformChanged;
            _box.SelectedContainerLayoutChanged += Box_SelectedContainerLayoutChanged;
            _box.SelectedContentSizeChanged += Box_SelectedContentSizeChanged;
            _box.SizeChanged += Box_SizeChanged;

            _pageControl = new BookCommandControl(_bookContext, _box);

            _bookMementoControl = new BookMementoControl(_book, BookHistoryCollection.Current);

            _box.Initialize();
            await WaitStableAsync(_box, token);

            RaisePropertyChanged(nameof(View));
            RaisePropertyChanged(null);
            PagesChanged?.Invoke(this, EventArgs.Empty);
            SelectedRangeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// ページ表示完了まで待機
        /// </summary>
        /// <param name="box"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task WaitStableAsync(PageFrameBox box, CancellationToken token)
        {
            if (!IsLoading()) return;

            using var eventFlag = new ManualResetEventSlim();
            using var viewContentChangedEvent = SubscribeViewContentChanged((s, e) => eventFlag.Set());
            while (IsLoading())
            {
                await eventFlag.WaitHandle.AsTask().WaitAsync(token);
                eventFlag.Reset();
            }

            bool IsLoading()
            {
                var pageFrameContent = box.GetSelectedPageFrameContent();
                if (pageFrameContent is null) return false;
                return pageFrameContent.GetViewContentState() < ViewContentState.Loaded;
            }
        }

        /// <summary>
        /// ブック安定まで待機
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitForBookStableAsync(CancellationToken token)
        {
            if (!_isLoading) return;

            using var eventFlag = new ManualResetEventSlim();
            using var loadingEvent = SubscribeLoading((s, e) => eventFlag.Set());
            while (_isLoading)
            {
                await eventFlag.WaitHandle.AsTask().WaitAsync(token);
            }
        }

        /// <summary>
        /// 表示ページ安定まで待機
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitForViewPageStableAsync(CancellationToken token)
        {
            await WaitForBookStableAsync(token);

            var box = _box;
            if (box is null) return;

            var boxToken = _openCancellationTokenSource.Token;

            try
            {
                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, boxToken);
                await WaitStableAsync(box, tokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                if (boxToken.IsCancellationRequested)
                {
                    await WaitForViewPageStableAsync(token);
                    return;
                }
                throw;
            }
        }



        private void Close()
        {
            if (_box is null) return;

            _bookMementoControl?.Dispose();
            _bookMementoControl = null;

            _pageControl?.Dispose();
            _pageControl = null;

            _box.PagesChanged -= Box_PagesChanged;
            _box.SelectedRangeChanged -= Box_SelectedRangeChanged;
            _box.PropertyChanged -= Box_PropertyChanged;
            _box.ViewContentChanged -= Box_ViewContentChanged;
            _box.TransformChanged -= Box_TransformChanged;
            _box.SelectedContainerLayoutChanged -= Box_SelectedContainerLayoutChanged;
            _box.SelectedContentSizeChanged -= Box_SelectedContentSizeChanged;
            _box.SizeChanged -= Box_SizeChanged;
            (_box as IDisposable)?.Dispose();
            _box = null;

            RaisePropertyChanged(nameof(View));

            Debug.Assert(_context is not null);
            //_bookContext.PagesChanged -= Box_PagesChanged;
            //_bookContext.SelectedRangeChanged -= Box_SelectedRangeChanged;
            //_bookContext.PropertyChanged -= Box_PropertyChanged;
            _context.Dispose();
            _context = null;

            Debug.Assert(_bookContext is not null);
            _bookContext.Dispose();
            _bookContext = null;

            Debug.Assert(_book is not null);
            _book = null; // Dispose は BookHub の仕事

            _viewPages = new List<Page>();

        }


        private List<Page> _viewPages = new();

        /// <summary>
        /// 安定した選択ページ
        /// </summary>
        public IReadOnlyList<Page> ViewPages => _viewPages;

        private void Box_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            ViewContentChanged?.Invoke(this, e);

            if (e.State < ViewContentState.Loaded) return;

            var pages = e.ViewContents.Select(e => e.Page).Distinct().ToList();
            if (_viewPages.SequenceEqual(pages)) return;

            _viewPages = pages;
            RaiseViewPageChanged(new ViewPageChangedEventArgs(_viewPages));
        }

        private readonly object _lock = new();
        private ViewPageChangedEventArgs? _viewPageChangedEventArgs;

        private void RaiseViewPageChanged()
        {
            lock (_lock)
            {
                if (_viewPageChangedEventArgs is null) return;
                RaiseViewPageChanged(_viewPageChangedEventArgs);
            }
        }

        private void RaiseViewPageChanged(ViewPageChangedEventArgs e)
        {
            lock (_lock)
            {
                _viewPageChangedEventArgs = e;
                if (!_isLoading)
                {
                    ViewPageChanged?.Invoke(this, _viewPageChangedEventArgs);
                    _viewPageChangedEventArgs = null;
                }
            }
        }

        private void Box_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            switch (e.Category)
            {
                case TransformCategory.Loupe:
                    ShowLoupeTransformMessage(e.Source, e.Action);
                    break;
                case TransformCategory.Content:
                    var originalScale = e is OriginalScaleTransformChangedEventArgs arg ? arg.OriginalScale : 1.0;
                    ShowContentTransformMessage(e.Source, e.Action, originalScale);
                    break;
            }

            TransformChanged?.Invoke(this, e);
        }

        // TODO: Selected の情報をまとめたクラスみたいなものがほしいかも？
        private void Box_SelectedContainerLayoutChanged(object? sender, EventArgs e)
        {
            SelectedContainerLayoutChanged?.Invoke(this, e);
        }

        private void Box_SelectedContentSizeChanged(object? sender, EventArgs e)
        {
            SelectedContentSizeChanged?.Invoke(this, e);
        }


        private void Box_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewSizeChanged?.Invoke(this, e);
        }



        private void Box_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

            switch (e.PropertyName)
            {
                case nameof(PageFrameBox.SelectedRange):
                    RaisePropertyChanged(nameof(SelectedRange));
                    break;
                case nameof(PageFrameBox.Pages):
                    RaisePropertyChanged(nameof(Pages));
                    break;
            }
        }

        private void Box_PagesChanged(object? sender, EventArgs e)
        {
            PagesChanged?.Invoke(this, e);
        }

        private void Box_SelectedRangeChanged(object? sender, EventArgs e)
        {
            SelectedRangeChanged?.Invoke(this, e);
        }

        public void ReOpen()
        {
            if (_context is null) return;

            _bookHub.RequestReLoad(this);
            //var memento = _bookContext.CreateMemento();
            //_bookHub.RequestLoad(memento);
        }


        private static void ShowLoupeTransformMessage(ITransformControlObject source, TransformAction action)
        {
            var infoMessage = InfoMessage.Current; // TODO: not singleton
            if (Config.Current.Notice.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            switch (action)
            {
                case TransformAction.Scale:
                    var scale = ((IScaleControl)source).Scale;
                    if (scale != 1.0)
                    {
                        infoMessage.SetMessage(InfoMessageType.ViewTransform, $"×{scale:0.0}");
                    }
                    break;
            }
        }

        private void ShowContentTransformMessage(ITransformControlObject source, TransformAction action, double originalScale)
        {
            var infoMessage = InfoMessage.Current; // TODO: not singleton
            if (Config.Current.Notice.ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            switch (action)
            {
                case TransformAction.Scale:
                    var scale = ((IScaleControl)source).Scale;
                    if (Config.Current.Notice.IsOriginalScaleShowMessage)
                    {
                        var dpi = (Window.GetWindow(this.View) is IDpiScaleProvider dpiProvider) ? dpiProvider.GetDpiScale().ToFixedScale().DpiScaleX : 1.0;
                        scale = scale * originalScale * dpi;
                    }
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(scale * 100.0 + 0.1)}%");
                    break;
                case TransformAction.Angle:
                    var angle = ((IAngleControl)source).Angle;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, $"{(int)(angle)}°");
                    break;
                case TransformAction.FlipHorizontal:
                    var isFlipHorizontal = ((IFlipControl)source).IsFlipHorizontal;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, Properties.Resources.Notice_FlipHorizontal + " " + (isFlipHorizontal ? "ON" : "OFF"));
                    break;
                case TransformAction.FlipVertical:
                    var isFlipVertical = ((IFlipControl)source).IsFlipVertical;
                    infoMessage.SetMessage(InfoMessageType.ViewTransform, Properties.Resources.Notice_FlipVertical + " " + (isFlipVertical ? "ON" : "OFF"));
                    break;
            }
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

        public DragTransformContext? CreateDragTransformContext(PageFrameContainer container, bool isLoupeTransform)
        {
            return _box?.CreateDragTransformContext(container, isLoupeTransform);
        }



        public void MoveTo(PagePosition position, LinkedListDirection direction)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.MoveTo(position, direction));
        }

        public void MoveToNextPage(LinkedListDirection direction)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.MoveToNextPage(direction));
        }

        public void MoveToNextFrame(LinkedListDirection direction)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.MoveToNextFrame(direction));
        }

        // 前のフォルダーに戻る
        public void MoveToNextFolder(LinkedListDirection direction, bool isShowMessage)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() =>
            {
                box.MoveToNextFolder(direction, isShowMessage);
            });


        }



        public void ScrollToNextFrame(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin)
        {
            var box = ValidBox();
            if (box is null) return;
            _pageControl?.Invoke(() => box.ScrollToNextFrame(direction, parameter, lineBreakStopMode, endMargin));
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


        /// <summary>
        /// 選択中の <see cref="PageFrameContent"/> を取得
        /// </summary>
        /// <returns></returns>
        public PageFrameContent? GetSelectedPageFrameContent()
        {
            return _box?.GetSelectedPageFrameContent();
        }

        /// <summary>
        /// 背景情報取得
        /// </summary>
        /// <returns></returns>
        public PageFrameBackground? GetBackground()
        {
            return _box?.GetBackground();
        }

        #endregion IPageFrameBox
    }
}
