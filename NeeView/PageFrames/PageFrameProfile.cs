using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NeeLaboratory.Generators;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame環境パラメータ
    /// TODO: PageFramesPanel 共通のパラメータのようなものにしたい
    /// </summary>
    [NotifyPropertyChanged]
    public partial class PageFrameProfile : INotifyPropertyChanged, IStaticFrame, IDisposable
    {
        private Config _config;
        private BookConfig _bookConfig;
        private BookSettingConfig _settingConfig;
        private Size _canvasSize;
        private DpiScale _dpiScale;
        private bool disposedValue;

        public PageFrameProfile(Config config)
        {
            _config = config;
            _bookConfig = _config.Book;
            _settingConfig = _config.BookSetting;

            _bookConfig.PropertyChanged += BookConfig_PropertyChanged;
            _settingConfig.PropertyChanged += BookSettingConfig_PropertyChanged;
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsStaticFrame => _settingConfig.PageMode != PageMode.LinearPage;

        public double FrameMargin => _config.Book.FrameSpace;

        public Size CanvasSize
        {
            get => _canvasSize;
            set => SetProperty(ref _canvasSize, value);
        }

        public DpiScale DpiScale
        {
            get => _dpiScale;
            set => SetProperty(ref _dpiScale, value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _bookConfig.PropertyChanged -= BookConfig_PropertyChanged;
                    _settingConfig.PropertyChanged -= BookSettingConfig_PropertyChanged;
                }
                disposedValue = true;
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
                case nameof(BookConfig.FrameSpace):
                    RaisePropertyChanged(nameof(FrameMargin));
                    break;
            }
        }

        private void BookSettingConfig_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(BookSettingConfig.PageMode):
                    RaisePropertyChanged(nameof(IsStaticFrame));
                    break;
            }
        }


    }
}
