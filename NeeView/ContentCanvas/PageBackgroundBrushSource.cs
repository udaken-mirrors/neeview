using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ページ背景ブラシ
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PageBackgroundSource : INotifyPropertyChanged, IDisposable
    {
        private readonly BackgroundConfig _backgroundConfig;
        private Brush? _pageBackgroundBrush = null;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public PageBackgroundSource()
        {
            _backgroundConfig = Config.Current.Background;

            _disposables.Add(_backgroundConfig.SubscribePropertyChanged(nameof(BackgroundConfig.PageBackgroundColor), (s, e) =>
            {
                Update();
            }));

            _disposables.Add(_backgroundConfig.SubscribePropertyChanged(nameof(BackgroundConfig.IsPageBackgroundChecker), (s, e) =>
            {
                Update();
            }));

            Update();
        }


        public Brush? Brush
        {
            get { return _pageBackgroundBrush; }
            set { SetProperty(ref _pageBackgroundBrush, value); }
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

        private void Update()
        {
            Brush = _backgroundConfig.PageBackgroundColor.A > 0
                ? _backgroundConfig.IsPageBackgroundChecker ? CanvasBackgroundSource.CreateCheckerBrush(_backgroundConfig.PageBackgroundColor) : new SolidColorBrush(_backgroundConfig.PageBackgroundColor)
                : null;
        }
    }
}
