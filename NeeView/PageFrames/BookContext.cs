using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    public interface IStaticFrame : INotifyPropertyChanged
    {
        public bool IsStaticFrame { get; }
        public Size CanvasSize { get; }
        public DpiScale DpiScale { get; }
    }

    public interface IContentSizeCalculatorProfie
    {
        public double ContentsSpace { get; }
        public PageStretchMode StretchMode { get; }
        public AutoRotateType AutoRotateType { get; }
        public bool AllowEnlarge { get; }
        public bool AllowReduce { get; }
        public Size CanvasSize { get; }
        public DpiScale DpiScale { get; }
    }

    /// <summary>
    /// ブック表示に必要な情報をまとめたもの
    /// </summary>
    [NotifyPropertyChanged]
    public partial class BookContext : INotifyPropertyChanged, IStaticFrame, IDisposable, IContentSizeCalculatorProfie, IBookPageContext
    {
        private readonly IBook _book;
        private readonly Config _config;
        //private readonly BookPageViewSetting _bookSetting;
        private readonly BookSettingConfig _bookSetting;
        private readonly PageFrameProfile _frameProfile;
        private double _loupeScale;
        private PageRange _selectedRange;
        private DisposableCollection _disposables = new DisposableCollection();
        private bool _disposedValue;


        public static BookContext CreateDummyBookContext(Config config)
        {
            return new BookContext(new EmptyBook(), config); //, new BookPageViewSetting());
        }


        public BookContext(IBook book, Config config) //, BookPageViewSetting bookSetting)
        {
            _book = book;
            _config = config;
            //_bookSetting = bookSetting;
            _bookSetting = _config.BookSetting;

            _frameProfile = new PageFrameProfile(_config);
            _disposables.Add(_frameProfile);

            _loupeScale = _config.Loupe.DefaultScale;

            var startIndex = _book.Pages.FirstOrDefault(e => e.EntryName == book.Memento.Page)?.Index ?? 0;
            _selectedRange = new PageRange(new PagePosition(startIndex, 0), 2);

            _disposables.Add(_book.SubscribePagesChanged(Book_PagesChanged));
            _disposables.Add(_config.Book.SubscribePropertyChanged(BookConfig_PropertyChanged));
            _disposables.Add(_config.View.SubscribePropertyChanged(ViewConfig_PropertyChanged));
            _disposables.Add(_config.System.SubscribePropertyChanged(SystemConfig_PropertyChanged));
            _disposables.Add(_bookSetting.SubscribePropertyChanged(BookSetting_PropertyChanged));
            _disposables.Add(_frameProfile.SubscribePropertyChanged(FrameProfile_PropertyChanged));
            _disposables.Add(ImageResizeFilterConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageResizeFilterConfig))));
            _disposables.Add(ImageCustomSizeConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageCustomSizeConfig))));
            _disposables.Add(ImageTrimConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageTrimConfig))));
            _disposables.Add(ImageDotKeepConfig.SubscribePropertyChanged((s, e) => RaisePropertyChanged(nameof(ImageDotKeepConfig))));
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler<SizeChangedEventArgs>? SizeChanging;

        [Subscribable]
        public event EventHandler<SizeChangedEventArgs>? SizeChanged;

        [Subscribable]
        public event EventHandler? PagesChanged;

        [Subscribable]
        public event EventHandler<SelectedRangeChangedEventArgs>? SelectedRangeChanged;

        public bool IsEnabled => _book.Pages.Any();

        public IBook Book => _book;
        public IReadOnlyList<Page> Pages => _book.Pages;

        public PagePosition FirstPosition => Pages.Any() ? PagePosition.Zero : PagePosition.Empty;
        public PagePosition LastPosition => Pages.Any() ? new(Pages.Count - 1, 1) : PagePosition.Empty;

        public PageRange SelectedRange
        {
            get { return _selectedRange; }
            set { SetProperty(ref _selectedRange, value); }
        }

        public IReadOnlyList<Page> SelectedPages
        {
            get => SelectedRange.CollectPositions().Select(e => Pages[e.Index]).Distinct().ToList();
        }


        public BookMemoryService BookMemoryService => _book.BookMemoryService;

        public PageFrameOrientation FrameOrientation => _config.Book.Orientation;
        public double FrameMargin => _config.Book.FrameSpace;
        public double ContentsSpace => _config.Book.ContentsSpace;
        public PageStretchMode StretchMode => _config.View.StretchMode;
        public AutoRotateType AutoRotateType => _config.View.AutoRotate;
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
        public bool IsRecursiveFolder => _config.BookSetting.IsRecursiveFolder;
        public PageSortMode SortMode => _config.BookSetting.SortMode;

        public bool IsStaticFrame => _frameProfile.IsStaticFrame;
        public Size CanvasSize => _frameProfile.CanvasSize;
        public DpiScale DpiScale => _frameProfile.DpiScale;

        public ViewConfig ViewConfig => _config.View;
        public PerformanceConfig PerformanceConfig => _config.Performance;
        public ImageResizeFilterConfig ImageResizeFilterConfig => _config.ImageResizeFilter;
        public ImageCustomSizeConfig ImageCustomSizeConfig => _config.ImageCustomSize;
        public ImageTrimConfig ImageTrimConfig => _config.ImageTrim;
        public ImageDotKeepConfig ImageDotKeepConfig => _config.ImageDotKeep;


        public double LoupeScale
        {
            get { return _loupeScale; }
            set { SetProperty(ref _loupeScale, value); }
        }

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


        private void Book_PagesChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Pages));
            PagesChanged?.Invoke(this, e);
        }


        private void BookConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookConfig.Orientation):
                    RaisePropertyChanged(nameof(FrameOrientation));
                    break;

                case nameof(BookConfig.FrameSpace):
                    RaisePropertyChanged(nameof(FrameMargin));
                    break;

                case nameof(BookConfig.ContentsSpace):
                    RaisePropertyChanged(nameof(ContentsSpace));
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
                case nameof(BookPageViewSetting.PageMode):
                    RaisePropertyChanged(nameof(PageMode));
                    //RaisePropertyChanged(nameof(IsSupportedDividePage));
                    //RaisePropertyChanged(nameof(IsSupportedWidePage));
                    //RaisePropertyChanged(nameof(IsSupportedSingleFirstPage));
                    //RaisePropertyChanged(nameof(IsSupportedSingleLastPage));
                    break;

                case nameof(BookPageViewSetting.BookReadOrder):
                    RaisePropertyChanged(nameof(ReadOrder));
                    break;

                case nameof(BookPageViewSetting.IsSupportedDividePage):
                    RaisePropertyChanged(nameof(IsSupportedDividePage));
                    break;

                case nameof(BookPageViewSetting.IsSupportedWidePage):
                    RaisePropertyChanged(nameof(IsSupportedWidePage));
                    break;

                case nameof(BookPageViewSetting.IsSupportedSingleFirstPage):
                    RaisePropertyChanged(nameof(IsSupportedSingleFirstPage));
                    break;

                case nameof(BookPageViewSetting.IsSupportedSingleLastPage):
                    RaisePropertyChanged(nameof(IsSupportedSingleLastPage));
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
    }
}
