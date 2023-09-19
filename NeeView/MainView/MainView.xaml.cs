using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView : UserControl, IHasDeviceInput
    {
        private MainViewViewModel? _vm;
        private Window? _owner;
        private readonly DpiScaleProvider _dpiProvider = new();
        private readonly PageFrameBackground _background;


        public MainView()
        {
            InitializeComponent();

            _background = new PageFrameBackground(_dpiProvider);
            _background.SetBinding(PageFrameBackground.PageProperty, new Binding(nameof(MainViewViewModel.SelectedPage)));
            this.MainViewPanel.Children.Insert(0, _background);

            this.Loaded += MainView_Loaded;
            this.Unloaded += MainView_Unloaded;
            this.DataContextChanged += MainView_DataContextChanged;
        }


        [Subscribable]
        public event EventHandler? TransformChanged;


        public PageFrameBackground PageFrameBackground => _background;

        public MouseInput? MouseInput => _vm?.MouseInput;

        public TouchInput? TouchInput => _vm?.TouchInput;

        public DpiScaleProvider DpiProvider => _dpiProvider;


        public IDisposable SubscribePreviewKeyDown(KeyEventHandler handler)
        {
            PreviewKeyDown += handler;
            return new AnonymousDisposable(() => PreviewKeyDown -= handler);
        }

        public void Initialize()
        {
            _vm = this.DataContext as MainViewViewModel;
            if (_vm is null)
            {
                return;
            }

            ContentDropManager.Current.SetDragDropEvent(this.View);

            this.NowLoadingView.Source = NowLoading.Current;

            // Transform はここでは処理しない
#if false
            // render transform
            var transformView = new TransformGroup();
            transformView.Children.Add(_vm.ViewComponent.DragTransform.TransformView);
            transformView.Children.Add(_vm.ViewComponent.LoupeTransform.TransformView);
            this.MainContent.RenderTransform = transformView;
            this.MainContent.RenderTransformOrigin = new Point(0.5, 0.5);

            _transformCalc = new TransformGroup();
            _transformCalc.Children.Add(_vm.ViewComponent.DragTransform.TransformCalc);
            _transformCalc.Children.Add(_vm.ViewComponent.LoupeTransform.TransformCalc);
            _transformCalc.Changed += (s, e) => TransformChanged?.Invoke(s, e);
            this.MainContentShadow.RenderTransform = _transformCalc;
            this.MainContentShadow.RenderTransformOrigin = new Point(0.5, 0.5);
#endif

            _vm.ViewComponent.OpenContextMenuRequest += (s, e) => OpenContextMenu();
            _vm.ViewComponent.FocusMainViewRequest += (s, e) => FocusMainView();

            InitializeNonActiveTimer();
        }

        private void MainView_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is MainView control)
            {
                control.Initialize();
            }
        }


        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (_owner == window) return;

            SetOwnerWindow(window);

            var dpiScale = _owner is IDpiScaleProvider dpiProvider ? dpiProvider.GetDpiScale() : VisualTreeHelper.GetDpi(this);
            _dpiProvider.SetDipScale(dpiScale);
        }

        private void MainView_Unloaded(object sender, RoutedEventArgs e)
        {
            ResetOwnerWindow();
        }

        private void SetOwnerWindow(Window window)
        {
            _owner = window;
            _owner.Activated += Window_Activated;
            _owner.Deactivated += Window_Deactivated;
        }

        private void ResetOwnerWindow()
        {
            if (_owner != null)
            {
                _owner.Activated -= Window_Activated;
                _owner.Deactivated -= Window_Deactivated;
                _owner = null;
            }
        }

        private void Window_Activated(object? sender, EventArgs e)
        {
            SetCursorVisible(true);
        }

        private void Window_Deactivated(object? sender, EventArgs e)
        {
            SetCursorVisible(true);
        }


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            _dpiProvider.SetDipScale(newDpi);
        }


        private void FocusMainView()
        {
            this.View.Focus();
        }

        // TODO: 都度取得でなく、対象のコンテナを確定させてから開始しよう
        public void StretchWindow()
        {
            var window = Window.GetWindow(this);
            if (window is null) return;
            if (window.WindowState != WindowState.Normal) return;
            if (_vm is null) return;

            try
            {
                var canvasSize = new Size(this.MainViewCanvas.ActualWidth, this.MainViewCanvas.ActualHeight);
                var contentSize = GetContentRenderSize();
                if (contentSize.IsEmptyOrZero()) return;
                MainViewViewModel.StretchWindow(window, canvasSize, contentSize);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return;
            }
        }

        private static Size GetContentRenderSize()
        {
            var box = PageFrameBoxPresenter.Current.View;
            if (box is null) return Size.Empty;

            var pageFrameContent = box.GetSelectedPageFrameContent();
            if (pageFrameContent is null) return Size.Empty;

            return pageFrameContent.GetContentRect().Size;
        }


        #region タイマーによる非アクティブ監視

        // タイマーディスパッチ
        private DispatcherTimer? _nonActiveTimer;

        // 非アクティブ時間チェック用
        private DateTime _lastActionTime;
        private Point _lastActionPoint;
        private double _cursorMoveDistance;

        // 一定時間操作がなければカーソルを非表示にする仕組み
        // 初期化
        private void InitializeNonActiveTimer()
        {
            this.View.PreviewMouseMove += MainView_PreviewMouseMove;
            this.View.PreviewMouseDown += MainView_PreviewMouseAction;
            this.View.PreviewMouseUp += MainView_PreviewMouseAction;
            this.View.MouseEnter += MainView_MouseEnter;

            _nonActiveTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);
            //_nonActiveTimer = new DispatcherTimer();
            _nonActiveTimer.Interval = TimeSpan.FromSeconds(0.2);
            _nonActiveTimer.Tick += new EventHandler(DispatcherTimer_Tick);

            Config.Current.Mouse.AddPropertyChanged(nameof(MouseConfig.IsCursorHideEnabled), (s, e) => UpdateNonActiveTimerActivity());
            UpdateNonActiveTimerActivity();
        }

        private void UpdateNonActiveTimerActivity()
        {
            if (_nonActiveTimer is null) return;

            if (Config.Current.Mouse.IsCursorHideEnabled)
            {
                _nonActiveTimer.Start();
            }
            else
            {
                _nonActiveTimer.Stop();
            }

            SetCursorVisible(true);
        }

        // タイマー処理
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            // 非アクティブ時間が続いたらマウスカーソルを非表示にする
            if (IsCursorVisible() && (DateTime.Now - _lastActionTime).TotalSeconds > Config.Current.Mouse.CursorHideTime)
            {
                SetCursorVisible(false);
            }
        }
        // マウス移動
        private void MainView_PreviewMouseMove(object? sender, MouseEventArgs e)
        {
            var nowPoint = e.GetPosition(this.View);

            if (IsCursorVisible())
            {
                _cursorMoveDistance = 0.0;
            }
            else
            {
                _cursorMoveDistance += Math.Abs(nowPoint.X - _lastActionPoint.X) + Math.Abs(nowPoint.Y - _lastActionPoint.Y);
                if (_cursorMoveDistance > Config.Current.Mouse.CursorHideReleaseDistance)
                {
                    SetCursorVisible(true);
                }
            }

            _lastActionPoint = nowPoint;
            _lastActionTime = DateTime.Now;
        }

        // マウスアクション
        private void MainView_PreviewMouseAction(object? sender, MouseEventArgs e)
        {
            if (Config.Current.Mouse.IsCursorHideReleaseAction)
            {
                SetCursorVisible(true);
            }

            _cursorMoveDistance = 0.0;
            _lastActionTime = DateTime.Now;
        }

        // 表示領域にマウスが入った
        private void MainView_MouseEnter(object? sender, MouseEventArgs e)
        {
            SetCursorVisible(true);
        }

        // マウスカーソル表示ON/OFF
        private void SetCursorVisible(bool isVisible)
        {
            if (_vm is null) return;

            ////Debug.WriteLine($"Cursor: {isVisible}");
            _cursorMoveDistance = 0.0;
            _lastActionTime = DateTime.Now;

            isVisible = isVisible | !Config.Current.Mouse.IsCursorHideEnabled;
            if (isVisible)
            {
                if (this.View.Cursor == Cursors.None && !_vm.ViewComponent.IsLoupeMode)
                {
                    this.View.Cursor = null;
                }
            }
            else
            {
                if (this.View.Cursor == null)
                {
                    this.View.Cursor = Cursors.None;
                }
            }
        }

        /// <summary>
        /// カーソル表示判定
        /// </summary>
        private bool IsCursorVisible()
        {
            return this.View.Cursor != Cursors.None || _vm?.ViewComponent.IsLoupeMode == true;
        }

        #endregion タイマーによる非アクティブ監視

        #region ContextMenu

        public void OpenContextMenu()
        {
            if (this.MainViewPanel.ContextMenu != null)
            {
                _vm?.UpdateContextMenu();
                this.MainViewPanel.ContextMenu.DataContext = _vm;
                this.MainViewPanel.ContextMenu.PlacementTarget = this.MainViewPanel;
                this.MainViewPanel.ContextMenu.Placement = PlacementMode.MousePoint;
                this.MainViewPanel.ContextMenu.IsOpen = true;
            }
        }

        private void MainViewPanel_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _vm?.UpdateContextMenu();
        }

        #endregion ContextMenu

        #region SizeChanged

        private readonly object _windowSizeChangedLock = new();
        private bool _isResizeLocked;

        public void SetResizeLock(bool locked)
        {
            lock (_windowSizeChangedLock)
            {
                if (_isResizeLocked == locked) return;
                _isResizeLocked = locked;
            }
        }

        private void MainView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = Window.GetWindow(this);
            //var windowStateManager = ((window as IHasWindowController)?.WindowController.WindowStateManager) ?? throw new InvalidOperationException();

            // 最小化では処理しない
            if (window.WindowState == WindowState.Minimized) return;

            bool isResizeLocked;
            lock (_windowSizeChangedLock)
            {
                isResizeLocked = _isResizeLocked;
            }

            if (!isResizeLocked)
            {
                //Debug.WriteLine($"ViewSizeChange: {windowStateManager.CurrentState} {e.NewSize} ");
            }
            else
            {
                //Debug.WriteLine($"ViewSizeChange: Locked");
            }
        }

        #endregion SizeChanged
    }
}
