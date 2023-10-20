using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class MainViewManager
    {
        private static MainViewManager? _current;
        public static MainViewManager Current => _current ?? throw new InvalidOperationException();


        private MainViewWindow? _window;
        private readonly MainViewComponent _viewComponent;
        private readonly MainView _mainView;
        private readonly MainViewBay _mainViewBay;
        private IDisposableContent? _alternativeContent;
        private readonly ContentControl _defaultSocket;
        private bool _isStoreEnabled = true;
        private readonly MainViewLockerMediator _mediator;
        private readonly MainViewLocker _dockingLocker;
        private MainViewLocker? _floatingLocker;


        public static void Initialize(MainViewComponent viewComponent, ContentControl defaultSocket)
        {
            if (_current != null) throw new InvalidOperationException();
            _current = new MainViewManager(viewComponent, defaultSocket);
        }

        public MainViewManager(MainViewComponent viewComponent, ContentControl defaultSocket)
        {
            _viewComponent = viewComponent;

            _mainView = _viewComponent.MainView;
            _defaultSocket = defaultSocket;
            _mainViewBay = new MainViewBay();

            _defaultSocket.Content = _mainView;

            BookHub.Current.BookChanging += BookHub_BookChanging;

            Config.Current.MainView.AddPropertyChanged(nameof(MainViewConfig.IsFloating), (s, e) => Update());

            Config.Current.Panels.SubscribePropertyChanged(nameof(PanelsConfig.AlternativeContent), (s, e) => UpdateAlternativeContent());

            _mediator = new MainViewLockerMediator(_mainView);
            _dockingLocker = new MainViewLocker(_mediator, MainWindow.Current);
            _dockingLocker.Activate();
        }


        public MainViewWindow? Window => _window;
        public MainView MainView => _mainView;
        public MainViewBay MainViewBay => _mainViewBay;


        private void BookHub_BookChanging(object? sender, BookChangingEventArgs e)
        {
            _mainView.MouseInput?.Cancel();
            _mainViewBay.MouseInput.Cancel();
        }

        public bool IsFloating()
        {
            return Config.Current.MainView.IsFloating;
        }

        public void SetFloating(bool isFloating)
        {
            if (!_isStoreEnabled) return;

            Config.Current.MainView.IsFloating = isFloating;
        }

        public bool RecoveryFloating()
        {
            if (_window is not null)
            {
                // ウィンドウが最小化されていたら復元する
                if (_window.WindowState == WindowState.Minimized)
                {
                    _window.WindowState = WindowState.Normal;
                    return true;
                }
            }
            return false;
        }

        public void Update()
        {
            if (Config.Current.MainView.IsFloating)
            {
                Floating();
            }
            else
            {
                Docking();
            }
        }

        private void Floating()
        {
            if (_window != null)
            {
                _window.Focus();
                return;
            }

            if (!Config.Current.MainView.WindowPlacement.IsValid())
            {
                var point = _mainView.PointToScreen(new Point(0.0, 0.0));
                Config.Current.MainView.WindowPlacement = new WindowPlacement(WindowState.Normal, (int)point.X + 32, (int)point.Y + 32, (int)_mainView.ActualWidth, (int)_mainView.ActualHeight);
            }

            _dockingLocker.Deactivate();

            _alternativeContent = CreateAlternativeContent();
            _defaultSocket.Content = _alternativeContent.Content;

            InfoMessage.Current.ClearMessage(ShowMessageStyle.Normal);

            _window = new MainViewWindow();
            _window.MainViewSocket.Content = _mainView;

            _window.Closing += (s, e) => Store();
            _window.Closed += (s, e) => SetFloating(false);

            _window.Show();
            _window.Activate();

            _floatingLocker = new MainViewLocker(_mediator, _window);
            _floatingLocker.Activate();
        }

        private void Docking()
        {
            if (_window is null) return;

            if (_floatingLocker != null)
            {
                _floatingLocker.Deactivate();
                _floatingLocker.Dispose();
                _floatingLocker = null;
            }

            _window.Close();
            _window.Content = null;
            _window = null;

            _alternativeContent?.Dispose();
            _alternativeContent = null;

            // NOTE: コンテンツの差し替えでLoadedイベントが呼ばれないことがあるため、新規コントロールをはさむことで確実にLoadedイベントが呼ばれるようにする。
            _defaultSocket.Content = new ContentControl() { Content = _mainView, IsTabStop = false, Focusable = false };

            _dockingLocker.Activate();
        }

        private IDisposableContent CreateAlternativeContent()
        {
            switch (Config.Current.Panels.AlternativeContent)
            {
                case AlternativeContent.Space:
                    return new NormalAlternativeContent(_mainViewBay);
                case AlternativeContent.PageList:
                    return new LayoutPanelAlternativeContent(nameof(PageListPanel));
                default:
                    throw new NotSupportedException();
            }
        }

        private void UpdateAlternativeContent()
        {
            if (_window is null) return;

            _alternativeContent?.Dispose();
            _alternativeContent = CreateAlternativeContent();
            _defaultSocket.Content = _alternativeContent.Content;
        }


        public void SetIsStoreEnabled(bool allow)
        {
            _isStoreEnabled = allow;
        }

        public void Store()
        {
            if (!_isStoreEnabled) return;

            if (_window != null)
            {
                try
                {
                    Config.Current.MainView.WindowPlacement = _window.StoreWindowPlacement();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
