using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// リサイズによるコンテンツの再作成管理
    /// </summary>
    public class ContentRebuild : BindableBase, IDisposable
    {
        private MainViewComponent _viewComponent;
        private KeyPressWatcher _keyPressWatcher;
        private bool _isResizingWindow;
        private bool _isUpdateContentSize;
        private bool _isUpdateContentViewBox;
        private bool _isRequested;
        private bool _isBusy;
        private bool _isKeyUpChance;
        private bool _ignoreMouseState;
        private DisposableCollection _disposables = new DisposableCollection();


        public ContentRebuild(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;

            // コンテンツ変更監視
            _disposables.Add(_viewComponent.ContentCanvas.SubscribeContentChanged(
                (s, e) => Request()));
            _disposables.Add(_viewComponent.ContentCanvas.SubscribeContentSizeChanged(
                (s, e) => Request()));
            _disposables.Add(_viewComponent.ContentCanvas.SubscribeContentStretchChanged(
                (s, e) => RequestSoon()));

            // DPI変化に追従
            _disposables.Add(_viewComponent.MainView.DpiProvider.SubscribeDpiChanged(
                (s, e) => RequestWithResize()));

            // スケール変化に追従
            _disposables.Add(_viewComponent.DragTransform.SubscribePropertyChanged(nameof(DragTransform.Scale),
                (s, e) => Request()));

            // ルーペ状態に追従
            _disposables.Add(_viewComponent.LoupeTransform.SubscribePropertyChanged(nameof(LoupeTransform.FixedScale),
                (s, e) => Request()));

            // リサイズフィルター状態監視
            _disposables.Add(Config.Current.ImageResizeFilter.SubscribePropertyChanged(
                (s, e) => Request()));

            // ドット表示監視
            _disposables.Add(Config.Current.ImageDotKeep.SubscribePropertyChanged(nameof(ImageDotKeepConfig.IsEnabled),
                (s, e) => Request()));

            // サイズ指定状態監視
            _disposables.Add(Config.Current.ImageCustomSize.SubscribePropertyChanged(
                (s, e) => RequestWithResize()));
            _disposables.Add(Config.Current.ImageTrim.SubscribePropertyChanged(
                (s, e) => RequestWithTrim()));

            _disposables.Add(WindowMessage.Current.SubscribeEnterSizeMove(
                (s, e) => _isResizingWindow = true));
            _disposables.Add(WindowMessage.Current.SubscribeExitSizeMove(
                (s, e) => _isResizingWindow = false));

            // キー入力監視。メンバーなのでイベント解除不要
            _keyPressWatcher = new KeyPressWatcher(MainWindow.Current);
            _keyPressWatcher.PreviewKeyDown += (s, e) => _isKeyUpChance = false;
            _keyPressWatcher.PreviewKeyUp += (s, e) => _isKeyUpChance = true;

            Start();

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        /// <summary>
        /// 更新を停止させるために使用する
        /// </summary>
        public Locker Locker { get; } = new Locker();

        public bool IsRequested
        {
            get { return _isRequested; }
            set { if (_isRequested != value) { _isRequested = value; RaisePropertyChanged(); } }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { if (_isBusy != value) { _isBusy = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// フレーム処理
        /// 必要ならば現在の表示サイズでコンテンツを再作成する
        /// </summary>
        private void OnRendering(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            RebuildFrame();
        }

        private void RebuildFrame()
        {
            if (!_isRequested || _isResizingWindow || Locker.IsLocked) return;

            // サイズ指定による更新
            if (_isUpdateContentSize)
            {
                _isUpdateContentSize = false;
                _viewComponent.ContentCanvas.UpdateContentSize();
                _viewComponent.DragTransformControl.SnapView();
            }

            // トリミングによる更新
            if (_isUpdateContentViewBox)
            {
                _isUpdateContentSize = false;
                foreach (var viewConent in _viewComponent.ContentCanvas.CloneContents.Where(e => e.IsValid))
                {
                    viewConent.UpdateViewBox();
                }
            }

            if (!_isKeyUpChance && _keyPressWatcher.IsPressed) return;
            _isKeyUpChance = false;

            var mouseButtonBits = MouseButtonBitsExtensions.Create();
            if (_viewComponent.IsLoupeMode && Config.Current.Mouse.LongButtonDownMode == LongButtonDownMode.Loupe)
            {
                mouseButtonBits = MouseButtonBits.None;
            }
            if (!_ignoreMouseState && mouseButtonBits != MouseButtonBits.None) return;
            _ignoreMouseState = false;

            bool isSuccessed = true;
            var dpiScaleX = _viewComponent.MainView.DpiProvider.DpiScale.DpiScaleX;
            var scale = _viewComponent.DragTransform.Scale * _viewComponent.LoupeTransform.FixedScale * dpiScaleX;
            foreach (var viewConent in _viewComponent.ContentCanvas.CloneContents.Where(e => e.IsValid))
            {
                isSuccessed = viewConent.Rebuild(scale) && isSuccessed;
            }

            this.IsRequested = !isSuccessed;

            UpdateStatus();
        }

        // 更新要求
        public void Request()
        {
            if (_disposedValue) return;

            this.IsRequested = true;
        }

        // 即時更新要求
        public void RequestSoon()
        {
            if (_disposedValue) return;

            _ignoreMouseState = true;
            this.IsRequested = true;
        }

        // リサイズ更新要求
        public void RequestWithResize()
        {
            if (_disposedValue) return;

            _isUpdateContentSize = true;
            this.IsRequested = true;
        }

        // トリミング更新要求
        public void RequestWithTrim()
        {
            if (_disposedValue) return;

            _isUpdateContentSize = true;
            _isUpdateContentViewBox = true;
            this.IsRequested = true;
        }

        public void UpdateStatus()
        {
            if (_disposedValue) return;

            this.IsBusy = _viewComponent.ContentCanvas.CloneContents.Where(e => e.IsValid).Any(e => e.IsResizing);
        }

        private void Start()
        {
            if (_disposedValue) return;

            CompositionTarget.Rendering += OnRendering;
        }

        private void Stop()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Stop();
                    _keyPressWatcher.Dispose();
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }


}
