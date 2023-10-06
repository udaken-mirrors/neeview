using System;
using System.ComponentModel;
using System.Windows;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrameBox 状態情報
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PageFrameContext : INotifyPropertyChanged, IStaticFrame, IDisposable, IContentSizeCalculatorProfile
    {
        private readonly Config _config;
        private readonly BookSettingConfig _bookSetting;
        private readonly PageFrameProfile _frameProfile;
        private double _loupeScale;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;
        private readonly BookShareContext _share;
        private readonly BooleanLockValue _isSnapAnchor = new();
        private readonly ViewScrollContext _viewScrollContext;

        public PageFrameContext(Config config, BookShareContext share, ViewScrollContext viewScrollContext)
        {
            _config = config;
            _share = share;
            _viewScrollContext = viewScrollContext;
            _bookSetting = _config.BookSetting;

            _frameProfile = new PageFrameProfile(_config);
            _disposables.Add(_frameProfile);

            _loupeScale = _config.Loupe.DefaultScale;

            _disposables.Add(_config.Book.SubscribePropertyChanged(BookConfig_PropertyChanged));
            _disposables.Add(_config.View.SubscribePropertyChanged(ViewConfig_PropertyChanged));
            _disposables.Add(_config.System.SubscribePropertyChanged(SystemConfig_PropertyChanged));
            _disposables.Add(_bookSetting.SubscribePropertyChanged((s, e) => AppDispatcher.BeginInvoke(() => BookSetting_PropertyChanged(s, e))));
            _disposables.Add(_frameProfile.SubscribePropertyChanged(FrameProfile_PropertyChanged));
            _disposables.Add(ImageResizeFilterConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageResizeFilterConfig))));
            _disposables.Add(ImageCustomSizeConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageCustomSizeConfig))));
            _disposables.Add(ImageTrimConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageTrimConfig))));
            _disposables.Add(ImageDotKeepConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageDotKeepConfig))));
            _disposables.Add(() => _viewScrollContext.Clear());
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler<SizeChangedEventArgs>? SizeChanging;

        [Subscribable]
        public event EventHandler<SizeChangedEventArgs>? SizeChanged;


        public BookShareContext ShareContext => _share;
        public ViewScrollContext ViewScrollContext => _viewScrollContext;

        public PageFrameOrientation FrameOrientation => _config.Book.Orientation;
        public double FrameMargin => IsStaticFrame ? 1.0 : _config.Book.FrameSpace;

        // TODO: 更新イベントが余計に発生している？Propertyパターンにして抑制させることも可能(優先度低)
        public double ContentsSpace => _bookSetting.PageMode == PageMode.WidePage ? _config.Book.ContentsSpace : 0.0;
        public PageStretchMode StretchMode => _bookSetting.PageMode == PageMode.Panorama && _config.View.StretchMode == PageStretchMode.Uniform
            ? _config.Book.Orientation == PageFrameOrientation.Horizontal ? PageStretchMode.UniformToVertical : PageStretchMode.UniformToHorizontal
            : _config.View.StretchMode;

        public bool IsInsertDummyPage => _config.Book.IsInsertDummyPage;
        public AutoRotateType AutoRotateType => _config.View.AutoRotate;
        public bool AllowFileContentAutoRotate => _config.View.AllowFileContentAutoRotate;
        public bool AllowEnlarge => _config.View.AllowStretchScaleUp;
        public bool AllowReduce => _config.View.AllowStretchScaleDown;
        public bool IsFlipLocked => _config.View.IsKeepFlip;
        public bool IsScaleLocked => _config.View.IsKeepScale;
        public bool IsAngleLocked => _config.View.IsKeepAngle;
        public bool IsIgnoreImageDpi => _config.System.IsIgnoreImageDpi;


        public PageMode PageMode => _bookSetting.PageMode;
        public PageReadOrder ReadOrder => _bookSetting.BookReadOrder;
        public bool IsSupportedDividePage => _bookSetting.IsSupportedDividePage && _bookSetting.PageMode == PageMode.SinglePage;
        public bool IsSupportedWidePage => _bookSetting.IsSupportedWidePage && _bookSetting.PageMode == PageMode.WidePage;
        public bool IsSupportedSingleFirstPage => _bookSetting.IsSupportedSingleFirstPage && _bookSetting.PageMode == PageMode.WidePage;
        public bool IsSupportedSingleLastPage => _bookSetting.IsSupportedSingleLastPage && _bookSetting.PageMode == PageMode.WidePage;
        public bool IsRecursiveFolder => _bookSetting.IsRecursiveFolder;

        public bool IsStaticFrame => _frameProfile.IsStaticFrame;
        public Size CanvasSize => _frameProfile.CanvasSize;
        public DpiScale DpiScale => _frameProfile.DpiScale;

        public ViewConfig ViewConfig => _config.View;
        public PerformanceConfig PerformanceConfig => _config.Performance;
        public ImageResizeFilterConfig ImageResizeFilterConfig => _config.ImageResizeFilter;
        public ImageCustomSizeConfig ImageCustomSizeConfig => _config.ImageCustomSize;
        public ImageTrimConfig ImageTrimConfig => _config.ImageTrim;
        public ImageDotKeepConfig ImageDotKeepConfig => _config.ImageDotKeep;


        public TimeSpan ScrollDuration => TimeSpan.FromSeconds(_config.View.ScrollDuration);
        public TimeSpan PageChangeDuration => PageMode == PageMode.Panorama ? ScrollDuration : TimeSpan.FromSeconds(_config.View.PageMoveDuration);


        public double LoupeScale
        {
            get { return _loupeScale; }
            set { SetProperty(ref _loupeScale, value); }
        }


        public BooleanLockValue IsSnapAnchor => _isSnapAnchor;



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


        private void BookConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookConfig.Orientation):
                    RaisePropertyChanged(nameof(FrameOrientation));
                    RaisePropertyChanged(nameof(StretchMode));
                    break;

                case nameof(BookConfig.FrameSpace):
                    RaisePropertyChanged(nameof(FrameMargin));
                    break;

                case nameof(BookConfig.ContentsSpace):
                    RaisePropertyChanged(nameof(ContentsSpace));
                    break;

                case nameof(BookConfig.IsInsertDummyPage):
                    RaisePropertyChanged(nameof(IsInsertDummyPage));
                    break;
            }
        }

        private void ViewConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewConfig.StretchMode):
                    RaisePropertyChanged(nameof(StretchMode));
                    break;

                case nameof(ViewConfig.AutoRotate):
                    RaisePropertyChanged(nameof(AutoRotateType));
                    break;

                case nameof(ViewConfig.AllowFileContentAutoRotate):
                    RaisePropertyChanged(nameof(AllowFileContentAutoRotate));
                    break;

                case nameof(ViewConfig.AllowStretchScaleUp):
                    RaisePropertyChanged(nameof(AllowEnlarge));
                    break;

                case nameof(ViewConfig.AllowStretchScaleDown):
                    RaisePropertyChanged(nameof(AllowReduce));
                    break;

                case nameof(ViewConfig.IsKeepFlip):
                    RaisePropertyChanged(nameof(IsFlipLocked));
                    break;

                case nameof(ViewConfig.IsKeepScale):
                    RaisePropertyChanged(nameof(IsScaleLocked));
                    break;

                case nameof(ViewConfig.IsKeepAngle):
                    RaisePropertyChanged(nameof(IsAngleLocked));
                    break;
            }

            RaisePropertyChanged(nameof(ViewConfig));
        }

        private void SystemConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SystemConfig.IsIgnoreImageDpi):
                    RaisePropertyChanged(nameof(IsIgnoreImageDpi));
                    break;
            }
        }

        private void BookSetting_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookSettingConfig.PageMode):
                    RaisePropertyChanged(nameof(PageMode));
                    RaisePropertyChanged(nameof(StretchMode));
                    RaisePropertyChanged(nameof(ContentsSpace));
                    //RaisePropertyChanged(nameof(IsSupportedDividePage));
                    //RaisePropertyChanged(nameof(IsSupportedWidePage));
                    //RaisePropertyChanged(nameof(IsSupportedSingleFirstPage));
                    //RaisePropertyChanged(nameof(IsSupportedSingleLastPage));
                    break;

                case nameof(BookSettingConfig.BookReadOrder):
                    RaisePropertyChanged(nameof(ReadOrder));
                    break;

                case nameof(BookSettingConfig.IsSupportedDividePage):
                    RaisePropertyChanged(nameof(IsSupportedDividePage));
                    break;

                case nameof(BookSettingConfig.IsSupportedWidePage):
                    RaisePropertyChanged(nameof(IsSupportedWidePage));
                    break;

                case nameof(BookSettingConfig.IsSupportedSingleFirstPage):
                    RaisePropertyChanged(nameof(IsSupportedSingleFirstPage));
                    break;

                case nameof(BookSettingConfig.IsSupportedSingleLastPage):
                    RaisePropertyChanged(nameof(IsSupportedSingleLastPage));
                    break;

                //case nameof(BookSettingConfig.SortMode):
                //    RaisePropertyChanged(nameof(SortMode));
                //    break;

                case nameof(BookSettingConfig.IsRecursiveFolder):
                    RaisePropertyChanged(nameof(IsRecursiveFolder));
                    break;
            }
        }


        private void FrameProfile_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // NOTE: PageMode変更なので余計なイベントを発生させない
                // TODO: 原則すべてのイベントを発生させるべき。受け取り側で除外する。
                //case nameof(PageFrameProfile.IsStaticFrame):
                //    RaisePropertyChanged(nameof(IsStaticFrame));
                //    break;

                case nameof(PageFrameProfile.CanvasSize):
                    RaisePropertyChanged(nameof(CanvasSize));
                    break;

                case nameof(PageFrameProfile.DpiScale):
                    RaisePropertyChanged(nameof(DpiScale));
                    break;
            }
        }



        public void SetCanvasSize(object sender, SizeChangedEventArgs e)
        {
            SizeChanging?.Invoke(this, e);
            _frameProfile.CanvasSize = e.NewSize;
            SizeChanged?.Invoke(this, e);
        }

        public void SetDpiScale(DpiScale dpiScale)
        {
            _frameProfile.DpiScale = dpiScale;
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
