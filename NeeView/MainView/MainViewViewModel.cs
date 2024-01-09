//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeView.Effects;
using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class MainViewViewModel : BindableBase
    {
        private readonly MainViewComponent _viewComponent;
        private readonly PageFrameBoxPresenter _presenter;
        private readonly ContextMenu _contextMenu = new();
        private bool _isContextMenuDirty = true;
        private Visibility _busyVisibility = Visibility.Collapsed;
        private Page? _selectedPage;


        public MainViewViewModel(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
            _presenter = PageFrameBoxPresenter.Current;

            Config.Current.View.AddPropertyChanged(nameof(ViewConfig.MainViewMargin),
                (s, e) => RaisePropertyChanged(nameof(MainViewMargin)));

            // context menu
            ContextMenuManager.Current.AddPropertyChanged(nameof(ContextMenuManager.Current.SourceTree),
                (s, e) => SetContextMenuDirty());

            RoutedCommandTable.Current.Changed +=
                (s, e) => SetContextMenuDirty();

            // busy visibility
            //_viewComponent.ContentRebuild.AddPropertyChanged(nameof(ContentRebuild.IsBusy),
            //    (s, e) => UpdateBusyVisibility());

            BookOperation.Current.BookControl.AddPropertyChanged(nameof(IBookControl.IsBusy),
                (s, e) => UpdateBusyVisibility());

            _presenter.SubscribePropertyChanged(nameof(PageFrameBoxPresenter.IsLoading),
                (s, e) => UpdateBusyVisibility());

            _presenter.SubscribeViewContentChanged(Presenter_ViewContentChanged);
        }

        public MainViewComponent ViewComponent => _viewComponent;

        public PageFrameBoxPresenter PageFrameBoxPresenter => _presenter;

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

        public Page? SelectedPage
        {
            get { return _selectedPage; }
            set { SetProperty(ref _selectedPage, value); }
        }



        private void Presenter_ViewContentChanged(object? sender, FrameViewContentChangedEventArgs e)
        {
            if (e.State < ViewContentState.Loaded) return;

            var isStaticFrame = _presenter.View?.Context.IsStaticFrame ?? false;
            SelectedPage = isStaticFrame ? e.ViewContents.FirstOrDefault()?.Page : null;
        }

        private void UpdateBusyVisibility()
        {
            ////Debug.WriteLine($"IsBusy: {BookHub.Current.IsLoading}, {BookOperation.Current.IsBusy}, {ContentRebuild.Current.IsBusy}");
            //this.BusyVisibility = Config.Current.Notice.IsBusyMarkEnabled && (BookHub.Current.IsLoading || BookOperation.Current.BookControl.IsBusy || _viewComponent.ContentRebuild.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
            this.BusyVisibility = Config.Current.Notice.IsBusyMarkEnabled && (_presenter.IsLoading || BookOperation.Current.BookControl.IsBusy) && !SlideShow.Current.IsPlayingSlideShow ? Visibility.Visible : Visibility.Collapsed;
        }


        private void SetContextMenuDirty()
        {
            _isContextMenuDirty = true;
        }

        public void UpdateContextMenu()
        {
            if (!_isContextMenuDirty) return;
            _isContextMenuDirty = false;

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
        public static void StretchWindow(Window window, Size canvasSize, Size contentSize, bool adjustScale)
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

            var fixedSize = Config.Current.View.IsBaseScaleEnabled ? contentSize.Multi(1.0 / Config.Current.BookSetting.BaseScale) : contentSize;

            var limitSize = new Size(SystemParameters.VirtualScreenWidth - frameWidth, SystemParameters.VirtualScreenHeight - frameHeight);

            var box = PageFrameBoxPresenter.Current.View;
            if (box is null) return;

            var newCanvasSize = box.Context.StretchMode switch
            {
                PageStretchMode.Uniform or PageStretchMode.UniformToSize
                    => fixedSize.Limit(limitSize),
                _
                    => fixedSize.Clamp(limitSize),
            };
            window.Width = newCanvasSize.Width + frameWidth;
            window.Height = newCanvasSize.Height + frameHeight;

            StaticTrace($"StretchSize=({window.Width:f1},{window.Height:f1})");

            if (adjustScale)
            {
                box.ResetReferenceSize();
                box.Stretch(true, TransformTrigger.WindowSnap);
            }
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(s, args)}");
        }

        [Conditional("LOCAL_DEBUG")]
        private static void StaticTrace(string s, params object[] args)
        {
            Debug.WriteLine($"{nameof(MainViewViewModel)}: {string.Format(s, args)}");
        }
    }
}
