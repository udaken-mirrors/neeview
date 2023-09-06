using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Maths;
using NeeView.Windows;

namespace NeeView.PageFrames
{
    // TODO: ごちゃっとしてるので整備する
    // TODO: IsStaticFrame ON/OFF でのスクロール制御の違いが煩雑になっているので良くする
    // TODO: ContentControl になってるけどいいの？
    [NotifyPropertyChanged]
    public partial class PageFrameBox : Grid, INotifyPropertyChanged, IPageFrameBox, IDisposable, ICanvasToViewTranslator
    {
        private readonly PageFrameContainerCollection _containers;
        private readonly PageFrameScrollViewer _scrollViewer;
        private readonly PageFrameContainersCanvas _canvas;
        private readonly PageFrameContainersCleaner _cleaner;
        private readonly PageFrameContainersFiller _filler;
        private readonly BookContext _context;
        private readonly BookPageLoader _loader;
        private readonly ContentSizeCalculator _calculator;
        private readonly PageFrameContainersVisiblePageWatcher _visiblePageWatcher;
        private readonly PageFrameContainersViewBox _viewBox;
        private readonly PageFrameContainersCollectionRectMath _rectMath;
        private readonly PageFrameContainersLayout _layout;
        private readonly PageFrameCanvasPointStorage _canvasPointStorage;
        private readonly DpiScaleProvider _dpiScaleProvider;

        private readonly PageFrameTransformMap _transformMap;
        /// <summary>
        /// アンカーコンテナを座標補正する
        /// </summary>
        private readonly BooleanLockValue _isSnapAnchor = new();

        private readonly RepeatLimiter _scrollRepeatLimiter = new();
        private readonly ScrollLock _scrollLock = new();
        private readonly TransformControlFactory _transformControlFactory;
        //private MouseInput _mouseInput;

        private readonly SelectedContainer _selected;

        private bool _disposedValue;

        private readonly DisposableCollection _disposables = new();

        private readonly ViewSourceMap _viewSourceMap;
        private readonly PageFrameBackground _background;

        private readonly DragTransformContextFactory _dragTransformContextFactory;

        private readonly OnceDispatcher _onceDispatcher;



        public PageFrameBox(BookContext context)
        {
            _context = context;
            _disposables.Add(_context); // このクラスがDisposableする？
            _disposables.Add(_context.SubscribeSelectedRangeChanged((s, e) => SelectedRangeChanged?.Invoke(this, e)));
            _disposables.Add(_context.SubscribePropertyChanged((s, e) => AppDispatcher.BeginInvoke(() => Context_PropertyChanged(s, e))));
            _disposables.Add(_context.SubscribePagesChanged((s, e) => AppDispatcher.BeginInvoke(() => Context_PagesChanged(s, e))));
            _disposables.Add(_context.SubscribeSizeChanging(Context_SizeChanging));
            _disposables.Add(_context.SubscribeSizeChanged(Context_SizeChanged));

            _onceDispatcher = new();
            _disposables.Add(_onceDispatcher);

            _dpiScaleProvider = new DpiScaleProvider();
            _context.SetDpiScale(_dpiScaleProvider.DpiScale);
            _disposables.Add(_dpiScaleProvider.SubscribeDpiChanged((s, e) => _context.SetDpiScale(_dpiScaleProvider.DpiScale)));

            var loupeContext = new LoupeTransformContext(_context);
            var viewTransform = new PageFrameViewTransform(_context, loupeContext);
            _disposables.Add(viewTransform.SubscribeTransformChanged(ViewTransform_TransformChanged));
            _disposables.Add(viewTransform);

            _transformMap = new PageFrameTransformMap(_context.ShareContext);

            _calculator = new ContentSizeCalculator(_context);
            var frameFactory = new PageFrameFactory(_context, _calculator);
            _viewSourceMap = new ViewSourceMap(_context.BookMemoryService);
            var elementScaleFactory = new PageFrameElementScaleFactory(_context, _transformMap, loupeContext);
            _loader = new BookPageLoader(_context.Book, frameFactory, _viewSourceMap, elementScaleFactory, _context.BookMemoryService, _context.PerformanceConfig);
            _disposables.Add(_loader);
            _disposables.Add(_loader.SubscribePropertyChanged(nameof(BookPageLoader.IsBusy), (s, e) => RaisePropertyChanged(nameof(IsBusy))));

            var baseScaleTransform = new BaseScaleTransform(_context.ViewConfig);
            _disposables.Add(baseScaleTransform);
            var containerFactory = new PageFrameContainerFactory(_context, _transformMap, _viewSourceMap, loupeContext, baseScaleTransform);
            _containers = new PageFrameContainerCollection(frameFactory, containerFactory);
            _rectMath = new PageFrameContainersCollectionRectMath(_context, _containers);
            _layout = new PageFrameContainersLayout(_context, _containers);

            _canvas = new PageFrameContainersCanvas(_context, _containers);
            _scrollViewer = new PageFrameScrollViewer(_context, _canvas, viewTransform);
            _viewBox = new PageFrameContainersViewBox(_context, _scrollViewer);
            _disposables.Add(_viewBox);
            _cleaner = new PageFrameContainersCleaner(_context, _containers);
            _filler = new PageFrameContainersFiller(_context, _containers, _rectMath);
            _visiblePageWatcher = new PageFrameContainersVisiblePageWatcher(_context, _viewBox, _rectMath, _layout);

            _background = new PageFrameBackground(_dpiScaleProvider);
            _disposables.Add(_background);

            this.Children.Add(_background);

            var effectGrid = new Grid() { Name = "EffectLayer" };
            effectGrid.SetBinding(Grid.EffectProperty, new Binding(nameof(ImageEffect.Effect)) { Source = ImageEffect.Current });
            effectGrid.Children.Add(_scrollViewer);
            this.Children.Add(effectGrid);

            var viewContext = new ViewTransformContext(_context, _viewBox, _rectMath, _scrollViewer);
            _transformControlFactory = new TransformControlFactory(_context, viewContext, loupeContext, _scrollLock);


            _canvasPointStorage = new PageFrameCanvasPointStorage(_containers, viewContext);
            _dragTransformContextFactory = new DragTransformContextFactory(this, _transformControlFactory, Config.Current.View, Config.Current.Loupe);

            _selected = new SelectedContainer(_containers, SelectCenterNode);
            _disposables.Add(_selected);
            //_disposables.Add(_selected.SubscribePropertyChanged(nameof(_selected.PagePosition),
            //    (s, e) => _context.SetSelectedItem(_selected.PagePosition, false)));
            _disposables.Add(_selected.SubscribePropertyChanged(nameof(_selected.PagePosition),
                (s, e) => _context.SelectedRange = _selected.PageRange));
            _disposables.Add(_selected.SubscribePropertyChanged(nameof(_selected.Page),
                (s, e) => _background.SetPage(_context.IsStaticFrame ? _selected.Page : null)));
            _disposables.Add(_selected.SubscribeViewContentChanged(
                 (s, e) => ViewContentChanged?.Invoke(this, e)));

            _scrollViewer.SizeChanged += _context.SetCanvasSize;
            _containers.CollectionChanged += ContainerCollection_CollectionChanged;



            //_context.SelectedItemChanged += Context_SelectedItemChanged;
            _visiblePageWatcher.VisibleContainersChanged += VisiblePageWatcher_VisibleContainersChanged;
            _viewBox.RectChanging += ViewBox_RectChanging;
            _viewBox.RectChanged += ViewBox_RectChanged;

            // 表示ページ監視
            _disposables.Add(new PageFrameContainersSelectedPageWatcher(this, _context.Book));

            Loaded += PageFrameBox_Loaded;
        }


        public void Initialize()
        {
            var index = _context.Book.CurrentPage?.Index ?? 0;
            MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
            _scrollViewer.FlushScroll();
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        // ページ終端を超えて移動しようとした
        // 次の本への移動を要求
        [Subscribable]
        public event EventHandler<PageTerminatedEventArgs>? PageTerminated;

        [Subscribable]
        public event EventHandler<FrameViewContentChangedEventArgs>? ViewContentChanged;

        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;

        [Subscribable]
        public event EventHandler? SelectedContentSizeChanged;

        [Subscribable]
        public event EventHandler? SelectedContainerLayoutChanged;

        [Subscribable]
        public event EventHandler? SelectedRangeChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        public DragTransformContextFactory DragTransformContextFactory => _dragTransformContextFactory;


        public BookContext BookContext => _context;

        public Book Book => _context.Book;

        public IReadOnlyList<Page> Pages => _context.Pages;

        public PageRange SelectedRange => _context.SelectedRange;
        
        public bool IsBusy => _loader.IsBusy;


        public DragTransformContext? CreateDragTransformContext(bool isPointContainer, bool isLoupeTransform)
        {
            var pos = GetViewPosition();
            var node = isPointContainer ? GetPointedContainer(pos) : _selected.Node;
            if (node is null) return null;
            return CreateDragTransformContext(node, isLoupeTransform);
        }

        public DragTransformContext? CreateDragTransformContext(PageFrameContainer container, bool isLoupeTransform)
        {
            var node = _containers.Find(container);
            if (node is null) return null;
            return CreateDragTransformContext(node, isLoupeTransform);
        }

        private DragTransformContext? CreateDragTransformContext(LinkedListNode<PageFrameContainer> node, bool isLoupeTransform)
        {
            var dragContext = _dragTransformContextFactory.Create(node.Value, isLoupeTransform);
            dragContext.Initialize(GetViewPosition(), System.Environment.TickCount);
            SetControlContainer(node); // TODO: ここで指定していいの？
            return dragContext;
        }


        private Point GetViewPosition()
        {
            var point = Mouse.GetPosition(this);
            point.X -= this.ActualWidth * 0.5;
            point.Y -= this.ActualHeight * 0.5;
            return point;
        }



        private void PageFrameBox_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PageFrameBox_Loaded;
            InitializeDpiScaleProvider();

            //Initialize();
        }

        private void InitializeDpiScaleProvider()
        {
            _dpiScaleProvider.SetDipScale(VisualTreeHelper.GetDpi(this));
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _dpiScaleProvider.SetDipScale(newDpi);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _scrollViewer.SizeChanged -= _context.SetCanvasSize;
                    _containers.CollectionChanged -= ContainerCollection_CollectionChanged;
                    //_context.PropertyChanged -= Context_PropertyChanged;
                    //_context.SizeChanging -= Context_SizeChanging;
                    //_context.SizeChanged -= Context_SizeChanged;
                    //_context.PagesChanged -= Context_PagesChanged;
                    //_context.SelectedItemChanged -= Context_SelectedItemChanged;
                    _visiblePageWatcher.VisibleContainersChanged -= VisiblePageWatcher_VisibleContainersChanged;
                    _viewBox.RectChanging -= ViewBox_RectChanging;
                    _viewBox.RectChanged -= ViewBox_RectChanged;

                    _disposables.Dispose();

                    _containers.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }



        private void ContainerCollection_CollectionChanged(object? sender, PageFrameContainerCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case PageFrameContainerCollectionChangedEventAction.Add:
                case PageFrameContainerCollectionChangedEventAction.Remove:
                    if (!_selected.IsValid)
                    {
                        _selected.SetAuto();
                        AssertSelectedExists();
                    }
                    break;
                case PageFrameContainerCollectionChangedEventAction.UpdateTransform:
                    if (!_context.IsStaticFrame)
                    {
                        _onceDispatcher.BeginInvoke("UpdateContainersLayout", UpdateContainersLayout);
                    }
                    if (e.TransformChangedEventArgs is not null && _selected.Node == e.Node)
                    {
                        TransformChanged?.Invoke(this, e.TransformChangedEventArgs);
                    }
                    break;
                case PageFrameContainerCollectionChangedEventAction.UpdateContentSize:
                    //Debug.WriteLine($"# Container.ContentChanged: {e.Node.Value}");
                    UpdateContainers(e.Node);
                    AssertSelectedExists();
                    if (_selected.Node == e.Node)
                    {
                        SelectedContentSizeChanged?.Invoke(this, EventArgs.Empty);
                    }
                    break;

                case PageFrameContainerCollectionChangedEventAction.UpdateContainerLayout:
                    if (_selected.Node == e.Node)
                    {
                        SelectedContainerLayoutChanged?.Invoke(this, EventArgs.Empty);
                    }
                    break;
            }

            void UpdateContainersLayout()
            {
                //Debug.WriteLine($"# Container.TransformChanged: {e.Node.Value}");
                FillContainers();
                _layout.Flush();
                UpdatePosition();
                _isSnapAnchor.Reset();
                AssertSelectedExists();
            }

            void UpdateContainers(LinkedListNode<PageFrameContainer> node)
            {
                FillContainers();

                var options = (_isSnapAnchor.IsSet && node == _containers.Anchor.Node && _rectMath.WithinView(_viewBox.Rect, node))
                    ? ScrollToViewOriginOption.None
                    : ScrollToViewOriginOption.IgnoreFrameScroll;

                ScrollToViewOrigin(node, _containers.Anchor.Direction, options);
            }
        }

        //public PagePosition MaxItem
        //{
        //    get { return _context.LastPosition; }
        //}


        //public Rect ViewRect => _viewBox.Rect;


        private PageFrameCanvasPoint? _storePoint;


        private LinkedListNode<PageFrameContainer> SelectCenterNode()
        {
            return _rectMath.GetViewCenterContainer(_viewBox.Rect) ?? _containers.CollectNode().First();
        }

        private void Context_SizeChanging(object? sender, SizeChangedEventArgs e)
        {
            _containers.Anchor.Set(_rectMath.GetViewCenterContainer(_viewBox.Rect), _containers.Anchor.Direction);

            var node = _rectMath.GetViewCenterContainer(_viewBox.Rect);
            _storePoint = _canvasPointStorage.Store(node);
        }

        private void Context_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateContainers(PageFrameDartyLevel.Moderate);
        }


        private void Context_PagesChanged(object? sender, EventArgs e)
        {
            ResetContainers();
            PagesChanged?.Invoke(this, e);
        }

#if false
        private void Context_SelectedItemChanged(object? sender, SelectedItemChangedEventArgs e)
        {
            if (e.FromOutsize)
            {
                // 外部からの変更(スライダー？)なので即時座標反映させる
                MoveTo(_context.SelectedItem, LinkedListDirection.Next);
            }
        }
#endif

        /// <summary>
        /// すべてのコンテナを更新
        /// </summary>
        private void UpdateContainers(PageFrameDartyLevel level)
        {
            Debug.WriteLine($"# PageFrameBox.UpdateContainers(): IsSnap={_isSnapAnchor.IsSet}");

            _containers.SetDarty(level);
            _containers.Anchor.FixDirection();
            FillContainers();

            _canvasPointStorage.Restore(_storePoint);
            SnapView();

            _layout.Flush();
            _scrollViewer.FlushScroll();
            Cleanup();
        }

        /// <summary>
        /// すべてのコンテナを再作成
        /// </summary>
        private void ResetContainers()
        {
            using (var pauser = new BookPageLoadPause(_loader))
            {
                _containers.SetDarty(PageFrameDartyLevel.Replace);
                _transformMap.Clear();
                SetControlContainer(_selected.Node);
                _selected.Node.Value.ResetLayout();
                _layout.Layout(_selected.Node);

                var index = _selected.Node.Value.FrameRange.Min.Index;
                MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);

                _layout.Flush();
                _scrollViewer.FlushScroll();
                Cleanup();
            }
        }

        public void FlushLayout()
        {
            _layout.Flush();
            _scrollViewer.FlushScroll();
            Cleanup();
        }

        /// <summary>
        /// ViewRect 変更に伴う処理
        /// </summary>
        private void ViewBox_RectChanging(object? sender, RectChangeEventArgs e)
        {
        }

        private void ViewBox_RectChanged(object? sender, RectChangeEventArgs e)
        {
            FillContainers();
        }


        /// <summary>
        /// 環境パラメータの変更イベント処理
        /// </summary>
        private void Context_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_disposedValue) return;

            // TODO: プロパティによってどこまでリセットするか等を選別する
            // TODO: プロパティ変更による TransformMap のリセット

            var dirtyLevel = PageFrameDartyLevel.Moderate;

            switch (e.PropertyName)
            {
                case nameof(BookContext.SelectedRange):
                    RaisePropertyChanged(nameof(SelectedRange));
                    return;

                case nameof(BookContext.Pages):
                    RaisePropertyChanged(nameof(Pages));
                    return;

                //case nameof(BookContext.SelectedItem):
                //    RaisePropertyChanged(nameof(SelectedItem));
                //    return;

                case nameof(BookContext.IsStaticFrame):
                    _transformMap.ClearPoint(default);
                    break;

                case nameof(BookContext.FrameMargin):
                    FillContainers();
                    return;

                case nameof(BookContext.IsFlipLocked):
                    _transformMap.IsFlipLocked = _context.IsFlipLocked;
                    return;

                case nameof(BookContext.IsScaleLocked):
                    _transformMap.IsScaleLocked = _context.IsScaleLocked;
                    return;

                case nameof(BookContext.IsAngleLocked):
                    _transformMap.IsAngleLocked = _context.IsAngleLocked;
                    return;

                case nameof(BookContext.ReadOrder):
                    // ページ方向が切り替わったときの分割ページ位置補正
                    if (_context.PageMode == PageMode.SinglePage && _context.IsSupportedDividePage && _selected.Container.FrameRange.PartSize == 1)
                    {
                        MoveTo(_selected.PagePosition.OtherPart(), LinkedListDirection.Next);
                    }
                    break;

                case nameof(BookContext.IsSupportedDividePage):
                    // 分割ページでない場合の座標補正
                    if (!(_context.PageMode == PageMode.SinglePage && _context.IsSupportedDividePage))
                    {
                        MoveTo(_selected.PagePosition.Truncate(), LinkedListDirection.Next);
                    }
                    break;

                case nameof(BookContext.SortMode):
                    // 別処理
                    return;

                case nameof(BookContext.PageMode):
                case nameof(BookContext.FrameOrientation):
                    ResetContainers();
                    return;

                case nameof(BookContext.IsIgnoreImageDpi):
                case nameof(BookContext.DpiScale):
                case nameof(BookContext.ImageCustomSizeConfig):
                case nameof(BookContext.ImageTrimConfig):
                case nameof(BookContext.ImageResizeFilterConfig):
                case nameof(BookContext.ImageDotKeepConfig):
                case nameof(BookContext.IsInsertDummyPage):
                    dirtyLevel = PageFrameDartyLevel.Heavy;
                    break;

                case nameof(BookContext.ViewConfig):
                    return;

                case nameof(BookContext.CanvasSize):
                    return;
            }

            // すべてのコンテナを更新
            UpdateContainers(dirtyLevel);
        }

        private void VisiblePageWatcher_VisibleContainersChanged(object? sender, VisibleContainersChangedEventArgs e)
        {
            foreach (var container in _containers)
            {
                container.Activity.IsVisible = e.Containers.Contains(container);
            }

            _loader.RequestLoad(e.PageRange, e.Direction);
        }


        private void ViewTransform_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            _viewBox.UpdateViewRect();

            if (e.Category == TransformCategory.View && e.Action == TransformAction.Point)
            {
                UpdatePosition();
                _isSnapAnchor.Reset();
            }

            Cleanup(e.Category == TransformCategory.View);

            TransformChanged?.Invoke(this, e);
        }



        public void MoveTo(PagePosition position, LinkedListDirection direction)
        {
            if (!_context.IsEnabled) return;

            // TODO: position の範囲チェック

            _containers.Anchor.Set(_rectMath.GetViewCenterContainer(_viewBox.Rect), direction);
            var next = _containers.EnsureLatestContainerNode(position, direction);
            if (next is null) return;
            _filler.FillContainersWhenAligned(_viewBox.Rect, next, direction);
            _layout.Layout();
            _layout.Flush();
            _containers.Anchor.Set(next, direction);

            _selected.Set(next);

            ScrollToViewOrigin(next, direction);
            Cleanup();

            AssertSelectedExists();

            _scrollLock.Lock();
            _isSnapAnchor.Set();
        }


        /// <summary>
        /// コンテンツをフレーム中央にスクロール。フレームオーバーの場合は方向に依存する
        /// </summary>
        private void ContentScrollToViewOrigin(LinkedListNode<PageFrameContainer> node, LinkedListDirection direction)
        {
            if (!_context.IsStaticFrame) return;

            if (node?.Value.Content is not PageFrameContent) return;

            // TODO: ページ移動による初期位置パラメータの反映。なにもしないという設定も新しく追加

            var contentRect = node.Value.GetContentRect().Size.ToRect();
            var viewRect = _viewBox.Rect.Size.ToRect();
            var point = new FramePointMath(_context, contentRect, viewRect).GetStartPoint(direction);
            point.X = -point.X; // コンテンツ座標系に補正する
            point.Y = -point.Y;

            _transformMap.ElementAt(node.Value.FrameRange).SetPoint(point, TimeSpan.Zero);
        }

        /// <summary>
        /// コンテンツをフレーム中央にスクロール
        /// </summary>
        private void ContentScrollToCenter(LinkedListNode<PageFrameContainer> node)
        {
            if (!_context.IsStaticFrame) return;

            if (node?.Value.Content is not PageFrameContent) return;

            _transformMap.ElementAt(node.Value.FrameRange).SetPoint(default, TimeSpan.Zero);
        }


        public void MoveToNextPage(LinkedListDirection direction)
        {
            if (!_context.IsEnabled) return;

            if (_context.PageMode != PageMode.WidePage)
            {
                MoveToNextFrame(direction);
                return;
            }

            // TODO: MoveToNextFrame と共通点が多いのでまとめられないだろうか
            var current = _selected.Node;
            Debug.Assert(current is not null);
            if (current is null) return;

            var pos = new PagePosition(current.Value.FrameRange.Top(direction.ToSign()).Index + direction.ToSign(), direction == LinkedListDirection.Next ? 0 : 1);
            var next = _containers.EnsureLatestContainerNode(pos, direction);
            if (next?.Value.Content is not PageFrameContent)
            {
                PageTerminated?.Invoke(this, new PageTerminatedEventArgs(direction.ToSign()));
                return;
            }
            _containers.Anchor.Set(null, direction);
            _filler.FillContainersWhenAligned(_viewBox.Rect, next, direction);
            _layout.Layout();
            _layout.Flush();
            _containers.Anchor.Set(next, direction);

            _selected.Set(next);
            AssertSelectedExists();
            ScrollToViewOrigin(next, direction);
            _scrollViewer.FlushScroll();
            Cleanup();

            _scrollLock.Lock();
            _isSnapAnchor.Set();
        }

        public void MoveToNextFrame(LinkedListDirection direction)
        {
            if (!_context.IsEnabled) return;

            var current = _selected.Node;
            Debug.Assert(current is not null);
            if (current is null) return;

            var pos = current.Value.FrameRange.Next(direction.ToSign());
            var next = _containers.EnsureLatestContainerNode(pos, direction);
            if (next?.Value.Content is not PageFrameContent)
            {
                PageTerminated?.Invoke(this, new PageTerminatedEventArgs(direction.ToSign()));
                return;
            }
            _containers.Anchor.Set(null, direction);
            _filler.FillContainersWhenAligned(_viewBox.Rect, next, direction);
            _layout.Layout();
            _layout.Flush();
            _containers.Anchor.Set(next, direction);

            _selected.Set(next);
            AssertSelectedExists();
            ScrollToViewOrigin(next, direction);
            Cleanup();

            _scrollLock.Lock();
            _isSnapAnchor.Set();
        }

        public void MoveToNextFolder(LinkedListDirection direction, bool isShowMessage)
        {
            if (!_context.IsEnabled) return;

            var index = direction == LinkedListDirection.Previous
                ? _context.Book.Pages.GetPrevFolderIndex(_selected.PageRange.Min.Index)
                : _context.Book.Pages.GetNextFolderIndex(_selected.PageRange.Min.Index);
            if (index >= 0)
            {
                MoveTo(new PagePosition(index, 0), LinkedListDirection.Next);
            }

            ShowMoveFolderPageMessage(index, direction, isShowMessage);
        }

        // TODO: InfoMessage系ここで？
        private void ShowMoveFolderPageMessage(int index, LinkedListDirection direction, bool isShowMessage)
        {
            var terminateMessage = direction == LinkedListDirection.Previous
                ? Properties.Resources.Notice_FirstFolderPage
                : Properties.Resources.Notice_LastFolderPage;

            if (index < 0)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, terminateMessage);
            }
            else if (isShowMessage)
            {
                var directory = _context.Book.Pages[index].GetSmartDirectoryName();
                if (string.IsNullOrEmpty(directory))
                {
                    directory = "(Root)";
                }
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, directory);
            }
        }


        // Scroll + NextFrame
        public void ScrollToNextFrame(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin)
        {
            if (!_context.IsEnabled) return;

            var isTerminated = ScrollToNext(direction, parameter, lineBreakStopMode, endMargin);
            if (isTerminated)
            {
                MoveToNextFrame(direction);
            }
        }

        /// <summary>
        /// NScroll
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>is scroll terminated</returns>
        public bool ScrollToNext(LinkedListDirection direction, IScrollNTypeParameter parameter)
        {
            return ScrollToNext(direction, parameter, LineBreakStopMode.Line, 0.0);
        }

        /// <summary>
        /// NScroll
        /// </summary>
        /// <param name="direction"></param>
        /// <returns>is scroll terminated</returns>
        public bool ScrollToNext(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin)
        {
            if (!_context.IsEnabled) return true;

            //var lineBreakStopMode = LineBreakStopMode.Line; // TODO: このパラメータはどこから？
            //var parameter = new ScrollNTypeParameter(); // TODO: このパラメータはどこから？
            //parameter.ScrollType = NScrollType.NType;


            // TODO: NType以外のスクロール

            // line break repeat limiter
            var isLimit = _scrollRepeatLimiter.IsLimit((int)(parameter.LineBreakStopTime * 1000.0));
            _scrollRepeatLimiter.Reset();

            var node = _selected.Node;
            AssertSelectedExists();
            if (node?.Value.Content is not PageFrameContent) return false;
            var contentRect = _transformControlFactory.CreateContentRect(node.Value);
            var viewRect = _transformControlFactory.CreateViewRect(_viewBox.Rect);

            var math = new NScroll(_context, contentRect, viewRect);
            var scroll = math.ScrollN(direction.ToSign(), parameter, endMargin);

            if (scroll.IsLineBreak && isLimit && (scroll.IsTerminated || lineBreakStopMode == LineBreakStopMode.Line))
            {
                return false;
            }

            if (scroll.IsTerminated)
            {
                return true;
            }
            else
            {
                AddPosition(scroll.Vector.X, scroll.Vector.Y, TimeSpan.FromSeconds(parameter.ScrollDuration)); // TODO: parameter.ScrollDuration
                return false;
            }
        }


        /// <summary>
        /// コンテナを表示中央にスクロール。サイズオーバーする場合は方向指定で表示位置を決定する。
        /// </summary>
        private void ScrollIntoViewOrigin(LinkedListNode<PageFrameContainer> node, LinkedListDirection direction)
        {
            var point = new FramePointMath(_context, node.Value.Rect, _viewBox.Rect).GetStartPoint(direction);
            _scrollViewer.SetPoint(new Point(-point.X, -point.Y), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// コンテナを表示中央にスクロール
        /// </summary>
        private void ScrollIntoViewCenter(LinkedListNode<PageFrameContainer> node)
        {
            var point = new FramePointMath(_context, node.Value.Rect, _viewBox.Rect).GetCenterPoint();
            _scrollViewer.SetPoint(new Point(-point.X, -point.Y), TimeSpan.FromMilliseconds(500));
        }


        // TODO: now 引数はどうなのか？
        // - NScroll の時間パラメータ
        // - 連続判定によるカーブ指定
        public void AddPosition(double dx, double dy, TimeSpan span)
        {
            if (!_context.IsEnabled) return;

            var node = _selected.Node;
            AssertSelectedExists();
            if (node?.Value.Content is not PageFrameContent) return;

            SetControlContainer(node);
            //_transformMap.SetTarget(container.FrameRange);
            var transform = _transformControlFactory.Create(node.Value);

            var delta = new Vector(dx, dy);
            //var span = now ? TimeSpan.Zero : TimeSpan.FromMilliseconds(500);
            transform.SetPoint(transform.Point + delta, span);

            _selected.SetAuto();
            AssertSelectedExists();
            _isSnapAnchor.Reset();
            //UpdateTransform(node);
        }

#if false
        public void AddAngle(double delta)
        {
            if (!_context.IsEnabled) return;

            var node = _selected.Node;
            AssertSelectedExists();
            if (node?.Value.Content is not PageFrameContent) return;

            SetControlContainer(node);
            //_transformMap.SetTarget(container.FrameRange);
            var transform = _transformControlFactory.Create(node.Value);

            var span = TimeSpan.Zero;
            transform.SetAngle(transform.Angle + delta, span);

            _scrollLock.Unlock();

            _selected.SetAuto();
            AssertSelectedExists();
            _isSnapAnchor.Reset();
        }

        public void AddScale(double delta)
        {
            if (!_context.IsEnabled) return;

            var node = _selected.Node;
            AssertSelectedExists();
            if (node?.Value.Content is not PageFrameContent) return;

            SetControlContainer(node);
            var transform = _transformControlFactory.Create(node.Value);

            var span = TimeSpan.Zero;
            transform.SetScale(transform.Scale + delta, span);

            _scrollLock.Unlock();

            _selected.SetAuto();
            AssertSelectedExists();
            _isSnapAnchor.Reset();
        }
#endif

        /// <summary>
        /// コンテンツのスナップ処理
        /// </summary>
        private void SnapView()
        {
            if (!_selected.IsValid) return;
            SnapView(_selected.Node);
        }

        private void SnapView(LinkedListNode<PageFrameContainer> node)
        {
            if (node?.Value.Content is not PageFrameContent) return;

            var transform = _transformControlFactory.Create(node.Value);
            transform.SnapView();
        }



        public LinkedListNode<PageFrameContainer>? GetPointedContainer(Point point)
        {
            return _containers.Find(TranslateViewToCanvas(point), _context.FrameOrientation);
        }

        // 操作するコンテナを宣言
        public void SetControlContainer(LinkedListNode<PageFrameContainer> node)
        {
            _containers.Anchor.Set(node);
            node.Value.HorizontalAlignment = HorizontalAlignment.Center;
            node.Value.VerticalAlignment = VerticalAlignment.Center;
        }

        public Point TranslateCanvasToViewPoint(Point point)
        {
            var x = _scrollViewer.Point.X + point.X;
            var y = _scrollViewer.Point.Y + point.Y;
            return new Point(x, y);
        }

        public Point TranslateViewToCanvas(Point point)
        {
            var x = point.X - _scrollViewer.Point.X;
            var y = point.Y - _scrollViewer.Point.Y;
            return new Point(x, y);
        }

        /// <summary>
        /// 現在ページを画面中央のものに更新
        /// </summary>
        public void UpdatePosition()
        {
            try
            {
                _selected.SetAuto();
                AssertSelectedExists();
            }
            catch
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// 表示範囲に必要なコンテナを埋める
        /// </summary>
        private void FillContainers()
        {
            var anchor = _rectMath.GetViewAnchorContainer(_viewBox.Rect);
            if (anchor is null) return;
            _filler.FillContainers(_viewBox.Rect, anchor);
            _layout.Layout(anchor);
        }

        private void Cleanup(bool isUpdateSelected)
        {
            Cleanup();

            if (isUpdateSelected)
            {
                AssertSelectedExists();
            }
        }

        private void Cleanup()
        {
            _cleaner.Cleanup(_viewBox.ViewingRect);
        }


        public PageFrameTransformAccessor CreateSelectedTransform()
        {
            return _transformMap.CreateAccessor(_selected.PageRange);
        }

        public void ResetTransform()
        {
            _transformMap.Clear();
        }

        /// <summary>
        /// 表示領域にストレッチするようにスケール変更
        /// </summary>
        /// <param name="ignoreViewOrigin">ストレッチ後のコンテナ座標をViewOriginでなくCenterにする</param>
        public void Stretch(bool ignoreViewOrigin)
        {
            var node = _selected.Node;
            if (node?.Value.Content is not PageFrameContent content) return;

            SetControlContainer(node);

            var transform = content.Transform;

            var rawSize = content.PageFrame.Size; 

            var scale = _calculator.CalcStretchScale(rawSize, new RotateTransform(transform.Angle));
            transform.SetScale(scale, TimeSpan.Zero);

            var options = ignoreViewOrigin ? ScrollToViewOriginOption.IgnoreViewOrigin : ScrollToViewOriginOption.None;
            ScrollToViewOrigin(node, _containers.Anchor.Direction, options);
            _scrollViewer.FlushScroll();
        }


        [Flags]
        private enum ScrollToViewOriginOption
        {
            None = 0,
            IgnoreFrameScroll = (1 << 0),
            IgnoreViewOrigin = (1 << 1),
        }

        /// <summary>
        /// 対象を表示中央にスクロール。サイズオーバーする場合は方向指定で表示位置を決定する。
        /// </summary>
        private void ScrollToViewOrigin(LinkedListNode<PageFrameContainer> node, LinkedListDirection direction, ScrollToViewOriginOption options = ScrollToViewOriginOption.None)
        {
            if (_context.ViewConfig.IsViewStartPositionCenter || options.HasFlag(ScrollToViewOriginOption.IgnoreViewOrigin))
            {
                if (!options.HasFlag(ScrollToViewOriginOption.IgnoreFrameScroll))
                {
                    ScrollIntoViewCenter(node);
                }
                ContentScrollToCenter(node);
            }
            else
            {
                if (!options.HasFlag(ScrollToViewOriginOption.IgnoreFrameScroll))
                {
                    ScrollIntoViewOrigin(node, direction);
                }
                ContentScrollToViewOrigin(node, direction);
            }
        }


        // ## ここで Selected が不明瞭になるバグを捉えるつもり
        [Conditional("DEBUG")]
        private void AssertSelectedExists()
        {
            var node = _selected.Node;
            Debug.Assert(node?.Value.Content is PageFrameContent);
        }


        /// <summary>
        /// [開発用] リセット
        /// </summary>
        public void Reset()
        {
        }

        /// <summary>
        /// 選択しているページの <see cref="PageFrameContent"/> を取得する。
        /// </summary>
        /// <returns><see cref="PageFrameContent"/> 存在しない場合はNULL</returns>
        public PageFrameContent? GetSelectedPageFrameContent()
        {
            var node = _selected.Node;
            if (node?.Value.Content is PageFrameContent pageFrameContent)
            {
                return pageFrameContent;
            }
            return null;
        }

        /// <summary>
        /// 背景情報
        /// </summary>
        /// <returns></returns>
        public PageFrameBackground GetBackground()
        {
            return _background;
        }

        internal void RaisePageTerminatedEvent(object? sender, int direction)
        {
            PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(direction));
        }
    }

    public interface ICanvasToViewTranslator
    {
        // TODO: 汎用化
        //Point TranslatePoint(Point point, UIElement relativeTo);

        Point TranslateCanvasToViewPoint(Point point);
        Point TranslateViewToCanvas(Point point);
    }
}