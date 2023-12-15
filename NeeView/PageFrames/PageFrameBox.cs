﻿//#define LOCAL_DEBUG

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using NeeView.ComponentModel;
using NeeView.Effects;
using NeeView.Maths;
using NeeView.Windows;

namespace NeeView.PageFrames
{
    // TODO: ごちゃっとしてるので整備する
    // TODO: IsStaticFrame ON/OFF でのスクロール制御の違いが煩雑になっているので良くする
    [NotifyPropertyChanged]
    public partial class PageFrameBox : Grid, INotifyPropertyChanged, IDisposable, ICanvasToViewTranslator, IDragTransformContextFactory
    {
        private readonly PageFrameContainerCollection _containers;
        private readonly PageFrameScrollViewer _scrollViewer;
        private readonly PageFrameContainerCanvas _canvas;
        private readonly PageFrameContainerCleaner _cleaner;
        private readonly PageFrameContainerFiller _filler;
        private readonly PageFrameContext _context;
        private readonly BookContext _bookContext;
        private readonly BookPageLoader _loader;
        private readonly ContentSizeCalculator _calculator;
        private readonly PageFrameContainerVisiblePageWatcher _visiblePageWatcher;
        private readonly PageFrameContainerViewBox _viewBox;
        private readonly PageFrameContainerCollectionRectMath _rectMath;
        private readonly PageFrameContainerLayout _layout;
        private readonly DpiScaleProvider _dpiScaleProvider;
        private readonly PageFrameTransformMap _transformMap;
        private readonly RepeatLimiter _scrollRepeatLimiter = new();
        private readonly ScrollLock _scrollLock = new();
        private readonly TransformControlFactory _transformControlFactory;
        private readonly SelectedContainer _selected;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private readonly ViewSourceMap _viewSourceMap;
        private readonly DragTransformContextFactory _dragTransformContextFactory;
        private readonly OnceDispatcher _onceDispatcher;
        private bool _isStarted;

        public PageFrameBox(PageFrameContext context, BookContext bookContext)
        {
            _context = context;
            _bookContext = bookContext;
            _disposables.Add(_bookContext.SubscribePropertyChanged((s, e) => AppDispatcher.BeginInvoke(() => BookContext_PropertyChanged(s, e))));
            _disposables.Add(_bookContext.SubscribeSelectedRangeChanged((s, e) => SelectedRangeChanged?.Invoke(this, e)));
            _disposables.Add(_bookContext.SubscribePagesChanged((s, e) => AppDispatcher.BeginInvoke(() => Context_PagesChanged(s, e))));
            _disposables.Add(_bookContext.SubscribeIsSortBusyChanged((s, e) => AppDispatcher.BeginInvoke(() => this.Cursor = e.IsSortBusy ? Cursors.Wait : null)));

            _onceDispatcher = new();
            _disposables.Add(_onceDispatcher);

            _dpiScaleProvider = new DpiScaleProvider();
            _context.SetDpiScale(_dpiScaleProvider.DpiScale);
            _disposables.Add(_dpiScaleProvider.SubscribeDpiChanged((s, e) => _context.SetDpiScale(_dpiScaleProvider.DpiScale)));

            var loupeContext = new LoupeTransformContext(_context);
            var viewTransform = new PageFrameViewTransform(_context, loupeContext);
            _disposables.Add(viewTransform);
            _disposables.Add(viewTransform.SubscribeTransformChanged(ViewTransform_TransformChanged));
            _disposables.Add(viewTransform.SubscribeViewPointChanged(ViewTransform_ViewPointChanged));

            _transformMap = new PageFrameTransformMap(_context.ShareContext);

            _calculator = new ContentSizeCalculator(_context);
            var frameFactory = new PageFrameFactory(_context, _bookContext, _calculator);
            _viewSourceMap = new ViewSourceMap(_bookContext.BookMemoryService);
            var elementScaleFactory = new PageFrameElementScaleFactory(_context, _transformMap, loupeContext);
            _loader = new BookPageLoader(_bookContext, frameFactory, _viewSourceMap, elementScaleFactory, _bookContext.BookMemoryService, _context.PerformanceConfig);
            _disposables.Add(_loader);
            _disposables.Add(_loader.SubscribePropertyChanged(nameof(BookPageLoader.IsBusy), (s, e) => RaisePropertyChanged(nameof(IsBusy))));

            var baseScaleTransform = new BaseScaleTransform(_context);
            _disposables.Add(baseScaleTransform);
            var containerFactory = new PageFrameContainerFactory(_context, _transformMap, _viewSourceMap, loupeContext, baseScaleTransform);
            _containers = new PageFrameContainerCollection(_context, frameFactory, containerFactory);
            _rectMath = new PageFrameContainerCollectionRectMath(_context, _containers);
            _layout = new PageFrameContainerLayout(_context, _containers);

            _canvas = new PageFrameContainerCanvas(_context, _containers);
            _scrollViewer = new PageFrameScrollViewer(_context, _canvas, viewTransform);
            _viewBox = new PageFrameContainerViewBox(_context, _scrollViewer);
            _disposables.Add(_viewBox);
            _cleaner = new PageFrameContainerCleaner(_context, _containers);
            _filler = new PageFrameContainerFiller(_context, _bookContext, _containers, _rectMath);
            _visiblePageWatcher = new PageFrameContainerVisiblePageWatcher(_context, _viewBox, _rectMath, _layout);

            var effectGrid = new Grid() { Name = "EffectLayer" };
            effectGrid.SetBinding(Grid.EffectProperty, new Binding(nameof(ImageEffect.Effect)) { Source = ImageEffect.Current });
            _disposables.Add(() => BindingOperations.ClearBinding(effectGrid, Grid.EffectProperty));
            effectGrid.Children.Add(_scrollViewer);
            this.Children.Add(effectGrid);

            var gridLine = new GridLine(ImageGridTarget.Screen) { Name = "ScreenGridLine" };
            _disposables.Add(gridLine);
            this.Children.Add(gridLine);

            var viewContext = new ViewTransformContext(_context, _viewBox, _rectMath, _scrollViewer);
            _transformControlFactory = new TransformControlFactory(_context, viewContext, loupeContext, _scrollLock);

            _dragTransformContextFactory = new DragTransformContextFactory(this, _transformControlFactory, Config.Current.View, Config.Current.Mouse, Config.Current.Loupe);

            _selected = new SelectedContainer(_containers, SelectCenterNode);
            _disposables.Add(_selected);
            _disposables.Add(_selected.SubscribePropertyChanged(nameof(_selected.PagePosition),
                (s, e) => _bookContext.SelectedRange = _selected.PageRange));
            _disposables.Add(_selected.SubscribeViewContentChanged(
                 (s, e) => ViewContentChanged?.Invoke(this, e)));

            _disposables.Add(_scrollViewer.SubscribeSizeChanged(_context.SetCanvasSize));
            _disposables.Add(_containers.SubscribeCollectionChanged(ContainerCollection_CollectionChanged));

            _disposables.Add(_visiblePageWatcher.SubscribeVisibleContainersChanged(VisiblePageWatcher_VisibleContainersChanged));

            // 表示ページ監視
            _disposables.Add(new PageFrameContainerSelectedPageWatcher(this, _bookContext.Book));

            Loaded += PageFrameBox_Loaded;
        }


        private void PageFrameBox_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PageFrameBox_Loaded;
            InitializeDpiScaleProvider();

            // ViewBox の変更イベント
            _disposables.Add(_viewBox.SubscribeRectChanging(ViewBox_RectChanging));
            _disposables.Add(_viewBox.SubscribeRectChanged(ViewBox_RectChanged));

            // Context のイベントは最後に処理
            _disposables.Add(_context.SubscribePropertyChanged((s, e) => AppDispatcher.BeginInvoke(() => Context_PropertyChanged(s, e))));
            _disposables.Add(_context.SubscribeSizeChanging(Context_SizeChanging));
            _disposables.Add(_context.SubscribeSizeChanged(Context_SizeChanged));

            // 最初のページ
            if (_bookContext.IsEnabled)
            {
                MoveTo(_bookContext.Book.StartPosition.Position, LinkedListDirectionExtensions.FromSign(_bookContext.Book.StartPosition.Direction));
                _scrollViewer.FlushScroll();
            }
            else
            {
                ViewContentChanged?.Invoke(this, new FrameViewContentChangedEventArgs(ViewContentChangedAction.ContentLoaded, null, Array.Empty<ViewContent>(), 1));
            }

            IsStarted = true;
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
        public event EventHandler<PageRangeChangedEventArgs>? SelectedRangeChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;


        public DragTransformContextFactory DragTransformContextFactory => _dragTransformContextFactory;

        public PageFrameContext Context => _context;
        public BookContext BookContext => _bookContext;

        public Book Book => _bookContext.Book;

        public IReadOnlyList<Page> Pages => _bookContext.Pages;

        public PageRange SelectedRange => _bookContext.SelectedRange;

        public bool IsBusy => _loader.IsBusy;

        public bool IsStarted
        {
            get { return _isStarted; }
            private set { SetProperty(ref _isStarted, value); }
        }

        // NOTE: 開発用に公開
        public ViewSourceMap ViewSourceMap => _viewSourceMap;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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

        private void InitializeDpiScaleProvider()
        {
            _dpiScaleProvider.SetDipScale(VisualTreeHelper.GetDpi(this));
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _dpiScaleProvider.SetDipScale(newDpi);
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
                ResetSnapAnchor();
                AssertSelectedExists();
            }

            void UpdateContainers(LinkedListNode<PageFrameContainer> node)
            {
                FillContainers();

                var options = (_context.IsSnapAnchor.IsSet && node == _containers.Anchor.Node && _rectMath.WithinView(_viewBox.Rect, node))
                    ? ScrollToViewOriginOption.None
                    : ScrollToViewOriginOption.IgnoreFrameScroll;

                ScrollToViewOrigin(node, _containers.Anchor.Direction, options);
            }
        }

        private LinkedListNode<PageFrameContainer> SelectCenterNode()
        {
            return _rectMath.GetViewCenterContainer(_viewBox.Rect) ?? _containers.CollectNode().First();
        }

        private void Context_SizeChanging(object? sender, SizeChangedEventArgs e)
        {
            _containers.Anchor.Set(_rectMath.GetViewCenterContainer(_viewBox.Rect), _containers.Anchor.Direction);
        }

        private void Context_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            UpdateContainers(PageFrameDirtyLevel.Moderate, TransformMask.None, false, false);
        }

        private void Context_PagesChanged(object? sender, EventArgs e)
        {
            _containers.SetDirty(PageFrameDirtyLevel.Moderate);

            // ページ座標復元
            var position = PagePosition.Zero;
            var page = _selected.Page;
            if (page is not null)
            {
                var index = _bookContext.Pages.IndexOf(page);
                position = new PagePosition(index < 0 ? 0 : index, 0);
            }

            MoveTo(position, LinkedListDirection.Next);
            FlushLayout();

            PagesChanged?.Invoke(this, e);
        }

        /// <summary>
        /// コンテナ更新
        /// </summary>
        /// <param name="dirtyLevel">コンテナ更新レベル</param>
        /// <param name="resetTransform">トランスフォームのリセットマスク</param>
        /// <param name="resetLayout">コンテナ配置初期化</param>
        /// <param name="snapOrigin">選択コンテナの開始位置スナップ</param>
        private void UpdateContainers(PageFrameDirtyLevel dirtyLevel, TransformMask resetTransform, bool resetLayout, bool snapOrigin)
        {
            using var pause = new BookPageLoadPause(_loader);

            // Transform 初期化
            if (resetTransform != TransformMask.None)
            {
                _transformMap.Clear(resetTransform);
            }

            // スナップポリシー更新
            if (snapOrigin)
            {
                _context.IsSnapAnchor.Set();
            }

            // アンカーを選択コンテナにする
            var node = _selected.Node;
            _containers.Anchor.Set(node);

            // コンテナにダーティーフラグを設定
            _containers.SetDirty(dirtyLevel);

            // 選択コンテナ更新
            _containers.UpdateContainer(node);

            // 選択コンテナ配置初期化
            if (resetLayout)
            {
                node.Value.ResetLayout();
            }

            // 座標補正
            if (_context.IsSnapAnchor.IsSet)
            {
                // 基準位置になるようスクロール
                ScrollToViewOrigin(node, _containers.Anchor.Direction);
            }
            else
            {
                // 表示範囲内補正
                SnapView();
            }

            // 表示に必要なコンテナ生成と配置
            FillContainers();

            // 表示即時反映
            FlushLayout();
        }

        /// <summary>
        /// 表示即時反映
        /// </summary>
        /// <remarks>
        /// スクロールを省略する
        /// </remarks>
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
            _containers.SetDirty(PageFrameDirtyLevel.Moderate);
            FillContainers();
        }


        private void BookContext_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(_bookContext.SelectedRange):
                    RaisePropertyChanged(nameof(SelectedRange));
                    return;

                case nameof(_bookContext.Pages):
                    RaisePropertyChanged(nameof(Pages));
                    return;
            }
        }

        /// <summary>
        /// 環境パラメータの変更イベント処理
        /// </summary>
        private void Context_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_disposedValue) return;

            switch (e.PropertyName)
            {
                case nameof(Context.ContentsSpace):
                    UpdateContainers(PageFrameDirtyLevel.Moderate, TransformMask.None, false, false);
                    break;

                case nameof(Context.FrameMargin):
                    FillContainers();
                    break;

                case nameof(Context.IsFlipLocked):
                    _transformMap.IsFlipLocked = _context.IsFlipLocked;
                    break;

                case nameof(Context.IsScaleLocked):
                    _transformMap.IsScaleLocked = _context.IsScaleLocked;
                    break;

                case nameof(Context.IsAngleLocked):
                    _transformMap.IsAngleLocked = _context.IsAngleLocked;
                    break;

                case nameof(Context.ReadOrder):
                    MoveToOtherPosition();
                    UpdateContainers(PageFrameDirtyLevel.Moderate, TransformMask.None, false, false);
                    break;

                case nameof(Context.IsSupportedDividePage):
                    MoveToFixPosition();
                    Flush();
                    break;

                case nameof(Context.IsSupportedSingleFirstPage):
                case nameof(Context.IsSupportedSingleLastPage):
                case nameof(Context.IsSupportedWidePage):
                    UpdateContainers(PageFrameDirtyLevel.Replace, TransformMask.None, true, true);
                    break;

                case nameof(Context.StretchMode):
                case nameof(Context.AllowEnlarge):
                case nameof(Context.AllowReduce):
                    UpdateContainers(PageFrameDirtyLevel.Moderate, TransformMask.Scale, false, true);
                    break;

                case nameof(Context.AutoRotate):
                case nameof(Context.AllowFileContentAutoRotate):
                    UpdateContainers(PageFrameDirtyLevel.Moderate, TransformMask.Angle, false, true);
                    break;

                case nameof(Context.FrameOrientation):
                    UpdateContainers(PageFrameDirtyLevel.Moderate, TransformMask.None, true, true);
                    break;

                case nameof(Context.PageMode):
                case nameof(Context.FramePageSize):
                case nameof(Context.IsPanorama):
                    MoveToFixPosition();
                    UpdateContainers(PageFrameDirtyLevel.Replace, TransformMask.Point, true, true);
                    break;

                case nameof(Context.IsLoopPage):
                    MoveToNormalPosition();
                    break;

                case nameof(Context.IsIgnoreImageDpi):
                case nameof(Context.DpiScale):
                case nameof(Context.ImageCustomSizeConfig):
                case nameof(Context.ImageTrimConfig):
                case nameof(Context.ImageResizeFilterConfig):
                case nameof(Context.ImageDotKeepConfig):
                case nameof(Context.IsInsertDummyPage):
                case nameof(Context.IsInsertDummyFirstPage):
                case nameof(Context.IsInsertDummyLastPage):
                    UpdateContainers(PageFrameDirtyLevel.Heavy, TransformMask.None, false, false);
                    break;
            }
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
                //_isSnapAnchor.Reset();
            }

            Cleanup(e.Category == TransformCategory.View);

            TransformChanged?.Invoke(this, e);
        }

        private void ViewTransform_ViewPointChanged(object? sender, EventArgs e)
        {
            if (_context.IsStaticFrame) return;

            var c0 = new Point(_containers.FirstTerminate.X, _containers.FirstTerminate.Y);
            var c1 = new Point(_containers.LastTerminate.X, _containers.LastTerminate.Y);

            if (_context.FrameOrientation == PageFrameOrientation.Horizontal)
            {
                if (_context.ReadOrder == PageReadOrder.RightToLeft)
                {
                    (c0, c1) = (c1, c0);
                }
            }

            _scrollViewer.ApplyAreaLimit(c0, c1);
        }

        private void SetSnapAnchor()
        {
            _context.IsSnapAnchor.Set();
        }

        public void ResetSnapAnchor()
        {
            _context.IsSnapAnchor.Reset();
        }

        /// <summary>
        /// ページ設定変更による不正ページ位置を補正
        /// </summary>
        private void MoveToFixPosition()
        {
            // NOTE: 切り替えるときは常にページ先頭になる。分割ページから分割ページに切り替わることはないはず
            _containers.SetDirty(PageFrameDirtyLevel.Moderate);
            MoveTo(_selected.PagePosition.Truncate(), LinkedListDirection.Next);
        }

        /// <summary>
        /// ページ方向が切り替わったときの分割ページ位置補正
        /// </summary>
        private void MoveToOtherPosition()
        {
            if (_context.IsSupportedDividePage && _selected.Container.FrameRange.PartSize == 1)
            {
                MoveTo(_selected.PagePosition.OtherPart(), LinkedListDirection.Next);
            }
        }

        /// <summary>
        /// ループ設定変更による正規ページ位置に補正
        /// </summary>
        private void MoveToNormalPosition()
        {
            if (_selected.Node.Value.Content is not PageFrameContent) return;

            var selectedPosition = _selected.Node.Value.Identifier;
            var position = _bookContext.NormalizePosition(selectedPosition);
            MoveTo(position, LinkedListDirection.Next);
            FlushLayout();
        }

        public void MoveTo(PagePosition position, LinkedListDirection direction)
        {
            if (!_bookContext.IsEnabled) return;

            //Debug.WriteLine($"MoveTo: Position={position}, Direction={direction}");

            // position の補正
            if (_context.IsLoopPage && _selected.Node.Value.Content is PageFrameContent)
            {
                var selectedIndex = _selected.Node.Value.Identifier.Index;
                var normalIndex = _bookContext.NormalizeIndex(selectedIndex);
                var diff = position.Index - normalIndex;
                position = new PagePosition(selectedIndex + diff, position.Part);
            }
            else
            {
                if (position < _bookContext.FirstPosition)
                {
                    position = _bookContext.FirstPosition;
                    direction = LinkedListDirection.Next;
                }
                else if (position > _bookContext.LastPosition)
                {
                    position = _bookContext.LastPosition;
                    direction = LinkedListDirection.Previous;
                }
            }

            _containers.Anchor.Set(_rectMath.GetViewCenterContainer(_viewBox.Rect), direction);
            var next = _containers.EnsureLatestContainerNode(position, direction);
            if (next is null) return;

            _filler.FillContainersWhenAligned(_viewBox.Rect, next, direction);
            _layout.Layout();
            _layout.Flush();
            _containers.Anchor.Set(next, direction);

            _context.SetAutoStretchTarget(next.Value.FrameRange);
            _selected.Set(next, true);

            ScrollToViewOrigin(next, direction);
            Cleanup();

            AssertSelectedExists();

            _scrollLock.Lock();
            SetSnapAnchor();
        }

        /// <summary>
        /// コンテンツをフレーム中央にスクロール。フレームオーバーの場合は方向に依存する
        /// </summary>
        private void ContentScrollToViewOrigin(LinkedListNode<PageFrameContainer> node, LinkedListDirection direction)
        {
            if (!_context.IsStaticFrame) return;

            if (node?.Value.Content is not PageFrameContent pageFrameContent) return;

            // TODO: ページ移動による初期位置パラメータの反映。なにもしないという設定も新しく追加

            var contentRect = node.Value.GetContentRect().Size.ToRect();
            var viewRect = _viewBox.Rect.Size.ToRect();
            var point = new FramePointMath(_context, contentRect, viewRect).GetStartPoint(direction);
            point.X = -point.X; // コンテンツ座標系に補正する
            point.Y = -point.Y;

            var key = PageFrameTransformTool.CreateKey(pageFrameContent.PageFrame);
            _transformMap.ElementAt(key).SetPoint(point, TimeSpan.Zero);
        }

        /// <summary>
        /// コンテンツをフレーム中央にスクロール
        /// </summary>
        private void ContentScrollToCenter(LinkedListNode<PageFrameContainer> node)
        {
            if (!_context.IsStaticFrame) return;

            if (node?.Value.Content is not PageFrameContent pageFrameContent) return;

            var key = PageFrameTransformTool.CreateKey(pageFrameContent.PageFrame);
            _transformMap.ElementAt(key).SetPoint(default, TimeSpan.Zero);
        }

        public void MoveToNextPage(LinkedListDirection direction)
        {
            if (!_bookContext.IsEnabled) return;

            if (_context.FramePageSize == 1)
            {
                MoveToNextFrame(direction);
                return;
            }

            var current = _selected.Node;
            Debug.Assert(current is not null);
            if (current is null) return;

            if (!BookProfile.Current.CanPrioritizePageMove() && IsSelectedPageFrameLoading())
            {
                return;
            }

            var nextIndex = current.Value.FrameRange.Min.Index + direction.ToSign();
            if (_context.IsLoopPage)
            {
                nextIndex = _bookContext.NormalizeIndex(nextIndex);
            }
            else if (!_bookContext.ContainsIndex(nextIndex))
            {
                PageTerminated?.Invoke(this, new PageTerminatedEventArgs(direction.ToSign()));
                return;
            }

            //Debug.WriteLine($"MoveToNextPage: {current.Value.FrameRange} to {nextIndex}");
            MoveTo(new PagePosition(nextIndex, 0), LinkedListDirection.Next);
            FlushLayout();
        }

        public void MoveToNextFrame(LinkedListDirection direction)
        {
            if (!_bookContext.IsEnabled) return;

            var current = _selected.Node;
            Debug.Assert(current is not null);
            if (current is null) return;

            if (!BookProfile.Current.CanPrioritizePageMove() && IsSelectedPageFrameLoading())
            {
                return;
            }

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

            _context.SetAutoStretchTarget(next.Value.FrameRange);
            _selected.Set(next, true);

            AssertSelectedExists();
            ScrollToViewOrigin(next, direction);
            Cleanup();

            _scrollLock.Lock();
            SetSnapAnchor();
        }

        public void MoveToNextFolder(LinkedListDirection direction, bool isShowMessage)
        {
            if (!_bookContext.IsEnabled) return;

            var index = direction == LinkedListDirection.Previous
                ? _bookContext.Book.Pages.GetPrevFolderIndex(_selected.PageRange.Min.Index)
                : _bookContext.Book.Pages.GetNextFolderIndex(_selected.PageRange.Min.Index);
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
                var directory = _bookContext.Book.Pages[index].GetSmartDirectoryName();
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
            if (!_bookContext.IsEnabled) return;

            var isTerminated = ScrollToNext(direction, parameter, lineBreakStopMode, endMargin);
            if (isTerminated)
            {
                MoveToNextFrame(direction);
            }
        }

        /// <summary>
        /// NScroll
        /// </summary>
        /// <param name="direction">scroll direction</param>
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
            if (!_bookContext.IsEnabled) return true;

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
                AddPosition(scroll.Vector.X, scroll.Vector.Y, _context.ScrollDuration);
                return false;
            }
        }

        /// <summary>
        /// コンテナを表示中央にスクロール。サイズオーバーする場合は方向指定で表示位置を決定する。
        /// </summary>
        private void ScrollIntoViewOrigin(LinkedListNode<PageFrameContainer> node, LinkedListDirection direction)
        {
            //Debug.WriteLine($"{node.Value}: {node.Value.Rect:f0} / {_viewBox.Rect:f0}");
            var point = new FramePointMath(_context, node.Value.Rect, _viewBox.Rect).GetStartPoint(direction);
            _scrollViewer.SetPoint(new Point(-point.X, -point.Y), _context.PageChangeDuration);
        }

        /// <summary>
        /// コンテナを表示中央にスクロール
        /// </summary>
        private void ScrollIntoViewCenter(LinkedListNode<PageFrameContainer> node)
        {
            var point = new FramePointMath(_context, node.Value.Rect, _viewBox.Rect).GetCenterPoint();
            _scrollViewer.SetPoint(new Point(-point.X, -point.Y), _context.PageChangeDuration);
        }


        // TODO: now 引数はどうなのか？
        // - NScroll の時間パラメータ
        // - 連続判定によるカーブ指定
        public void AddPosition(double dx, double dy, TimeSpan span)
        {
            if (!_bookContext.IsEnabled) return;

            var node = _selected.Node;
            AssertSelectedExists();
            if (node?.Value.Content is not PageFrameContent) return;

            SetControlContainer(node);
            var transform = _transformControlFactory.Create(node.Value);

            var delta = new Vector(dx, dy);
            transform.SetPoint(transform.Point + delta, span);

            _selected.SetAuto();
            AssertSelectedExists();
            ResetSnapAnchor();
        }

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

        /// <summary>
        /// 表示即時反映
        /// </summary>
        private void Flush()
        {
            _layout.Flush();
            _scrollViewer.FlushScroll();
            Cleanup();
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

        /// <summary>
        /// ViewContent を Dispose
        /// </summary>
        /// <remarks>
        /// ファイル削除するときに対応するページのリソースを開放するために使用。
        /// その後の継続した使用は考慮されていない。
        /// </remarks>
        public void DisposeViewContent(IEnumerable<Page> pages)
        {
            foreach (var content in _containers.Select(e => e.Content).OfType<PageFrameContent>().Where(e => pages.Any(x => e.PageFrame.Contains(x))))
            {
                content.DisposeViewContent();
            }
        }

        public PageFrameTransformAccessor CreateSelectedTransform()
        {
            if (_selected.Container.Content is PageFrameContent pageFrameContent)
            {
                var key = PageFrameTransformTool.CreateKey(pageFrameContent.PageFrame);
                return _transformMap.CreateAccessor(key);
            }
            else
            {
                return _transformMap.CreateAccessor(PageFrameTransformKey.Dummy);
            }
        }

        public void ResetTransform()
        {
            _transformMap.Clear();
        }

        /// <summary>
        /// リファレンスサイズを初期化
        /// </summary>
        public void ResetReferenceSize()
        {
            Context.ResetReferenceSize();
            UpdateContainers(PageFrameDirtyLevel.Moderate, TransformMask.None, false, false);
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
        /// 選択ページの読み込み中判定
        /// </summary>
        /// <returns></returns>
        public bool IsSelectedPageFrameLoading()
        {
            var pageFrameContent = GetSelectedPageFrameContent();
            if (pageFrameContent is null) return false;
            return pageFrameContent.GetViewContentState() < ViewContentState.Loaded;
        }

        /// <summary>
        /// ブック終端到達イベント発行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="direction">終端方向</param>
        /// <param name="isMedia">メディア操作での要求。ブックメディアとページメディアを区別するため</param>
        public void RaisePageTerminatedEvent(object? sender, int direction, bool isMedia)
        {
            if (isMedia && !_bookContext.IsMedia) return;
            PageTerminated?.Invoke(sender, new PageTerminatedEventArgs(direction));
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(s, args)}");
        }
    }

    public interface ICanvasToViewTranslator
    {
        Point TranslateCanvasToViewPoint(Point point);
        Point TranslateViewToCanvas(Point point);
    }
}