using NeeLaboratory.Generators;
using System.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// BookPageViewer, BookPageViewGenerator setting
    /// </summary>
    [NotifyPropertyChanged]
    public partial class BookPageViewSetting : INotifyPropertyChanged
    {
        private PageMode _pageMode;
        private PageReadOrder _bookReadOrder;
        private bool _isSupportedDividePage;
        private bool _isSupportedSingleFirstPage;
        private bool _isSupportedSingleLastPage;
        private bool _isSupportedWidePage;
        private AutoRotateType _autoRotate;


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        public PageMode PageMode
        {
            get { return _pageMode; }
            set { SetProperty(ref _pageMode, value); }
        }

        public PageReadOrder BookReadOrder
        {
            get { return _bookReadOrder; }
            set { SetProperty(ref _bookReadOrder, value); }
        }

        public bool IsSupportedDividePage
        {
            get { return _isSupportedDividePage; }
            set { SetProperty(ref _isSupportedDividePage, value); }
        }

        public bool IsSupportedSingleFirstPage
        {
            get { return _isSupportedSingleFirstPage; }
            set { SetProperty(ref _isSupportedSingleFirstPage, value); }
        }

        public bool IsSupportedSingleLastPage
        {
            get { return _isSupportedSingleLastPage; }
            set { SetProperty(ref _isSupportedSingleLastPage, value); }
        }

        public bool IsSupportedWidePage
        {
            get { return _isSupportedWidePage; }
            set { SetProperty(ref _isSupportedWidePage, value); }
        }

        public AutoRotateType AutoRotate
        {
            get { return _autoRotate; }
            set { SetProperty(ref _autoRotate, value); }
        }
    }
}
