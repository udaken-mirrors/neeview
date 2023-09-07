using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class MainViewViewModel : BindableBase
    {
        private readonly MainViewComponent _viewComponent;
        private readonly ContextMenu _contextMenu = new();
        private bool _isContextMenuDarty = true;
        private Visibility _busyVisibility = Visibility.Collapsed;


        public MainViewViewModel(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;

            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.MainViewMargin),
                (s, e) => RaisePropertyChanged(nameof(MainViewMargin)));

            // context menu
            ContextMenuManager.Current.AddPropertyChanged(nameof(ContextMenuManager.Current.SourceTree),
                (s, e) => SetContextMenuDarty());

            RoutedCommandTable.Current.Changed +=
                (s, e) => SetContextMenuDarty();

            // busy visibility
            //_viewComponent.ContentRebuild.AddPropertyChanged(nameof(ContentRebuild.IsBusy),
            //    (s, e) => UpdateBusyVisibility());

            BookOperation.Current.BookControl.AddPropertyChanged(nameof(IBookControl.IsBusy),
                (s, e) => UpdateBusyVisibility());

            PageFrameBoxPresenter.Current.SubscribePropertyChanged(nameof(PageFrameBoxPresenter.IsLoading),
                (s, e) => UpdateBusyVisibility());
        }


        public MainViewComponent ViewComponent => _viewComponent;

        public PageFrameBoxPresenter PageFrameBoxPresenter => PageFrameBoxPresenter.Current;

        //public ContentCanvas ContentCanvas => _viewComponent.ContentCanvas;

        //public ContentCanvasBrush ContentCanvasBrush => _viewComponent.ContentCanvasBrush;

        public ImageEffect ImageEffect => ImageEffect.Current;

        public WindowTitle WindowTitle => WindowTitle.Current;

        public InfoMessage InfoMessage => InfoMessage.Current;

        public LoupeContext LoupeContext => _viewComponent.LoupeContext;

        public MouseInput MouseInput => _viewComponent.MouseInput;

        public TouchInput TouchInput => _viewComponent.TouchInput;

        public Thickness MainViewMargin => new(Config.Current.View.MainViewMargin);

        public ContextMenu ContextMenu => _contextMenu;

        public Visibility BusyVisibility
        {
            get { return _busyVisibility; }
            set { if (_busyVisibility != value) { _busyVisibility = value; RaisePropertyChanged(); } }
        }



        private void UpdateBusyVisibility()
        {
            ////Debug.WriteLine($"IsBusy: {BookHub.Current.IsLoading}, {BookOperation.Current.IsBusy}, {ContentRebuild.Current.IsBusy}");
            //this.BusyVisibility = Config.Current.Notice.IsBusyMarkEnabled && (BookHub.Current.IsLoading || BookOperation.Current.BookControl.IsBusy || _viewComponent.ContentRebuild.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
            this.BusyVisibility = Config.Current.Notice.IsBusyMarkEnabled && (PageFrameBoxPresenter.Current.IsLoading || BookOperation.Current.BookControl.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
        }


        private void SetContextMenuDarty()
        {
            _isContextMenuDarty = true;
        }

        public void UpdateContextMenu()
        {
            if (!_isContextMenuDarty) return;
            _isContextMenuDarty = false;

            _contextMenu.Items.Clear();
            foreach (var item in ContextMenuManager.Current.CreateContextMenuItems())
            {
                _contextMenu.Items.Add(item);
            }

            _contextMenu.UpdateInputGestureText();
        }


        /// <summary>
        /// ウィンドウサイズをコンテンツサイズに合わせる
        /// </summary>
        /// <param name="window"></param>
        /// <param name="canvasSize"></param>
        /// <param name="contentSize"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void StretchWindow(Window window, Size canvasSize, Size contentSize)
        {
            if (contentSize.IsEmptyOrZero())
            {
                throw new ArgumentException($"canvasSize is 0.", nameof(canvasSize));
            }

            if (window.WindowState != WindowState.Normal)
            {
                throw new InvalidOperationException($"need Window.State is Normal");
            }

            var frameWidth = window.ActualWidth - canvasSize.Width;
            var frameHeight = window.ActualHeight - canvasSize.Height;
            if (frameWidth < 0.0 || frameHeight < 0.0)
            {
                throw new ArgumentException($"canvasSize must be smaller than Window.Size.", nameof(canvasSize));
            }

            var fixedSize = Config.Current.View.IsBaseScaleEnabled ? contentSize.Multi(1.0 / Config.Current.View.BaseScale) : contentSize;

            var limitSize = new Size(SystemParameters.VirtualScreenWidth - frameWidth, SystemParameters.VirtualScreenHeight - frameHeight);

            var box = PageFrameBoxPresenter.Current.View;
            if (box is null) return;

            var pageFrameContent = box.GetSelectedPageFrameContent();
            if (pageFrameContent is null) return;

            var baseAngle = pageFrameContent.PageFrame.Angle;

            var newCanvasSize = box.Context.StretchMode switch
            {
                PageStretchMode.Uniform or PageStretchMode.UniformToSize
                    => fixedSize.Limit(limitSize),
                _
                    => fixedSize.Clamp(limitSize),
            };
            window.Width = newCanvasSize.Width + frameWidth;
            window.Height = newCanvasSize.Height + frameHeight;

            // NOTE: レンダリングに回転を反映させるためにタイミングを遅らせる
            AppDispatcher.BeginInvoke(() =>
            {
                // 自動回転方向が変化したときの補正
                var pageFrameContent = box.GetSelectedPageFrameContent();
                if (pageFrameContent is null) return;
                var deltaAngle = pageFrameContent.PageFrame.Angle - baseAngle;
                if (Math.Abs(deltaAngle) > 1.0)
                {
                    var dragTransform = box.CreateDragTransformContext(false, false);
                    if (dragTransform is null) return;
                    var angle = dragTransform.Transform.Angle - deltaAngle;
                    dragTransform.Transform.SetAngle(angle, TimeSpan.Zero);
                }

                box.Stretch(ignoreViewOrigin: true);
            });
        }

    }
}
