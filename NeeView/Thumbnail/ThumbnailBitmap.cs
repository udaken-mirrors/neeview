using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Thumbnail の BitmapSource化
    /// </summary>
    [NotifyPropertyChanged]
    public partial class ThumbnailBitmap : INotifyPropertyChanged, IDisposable
    {
        private IThumbnail _thumbnail;
        private ImageSource? _imageSource;
        private bool _disposedValue;

        public ThumbnailBitmap(IThumbnail thumbnail)
        {
            _thumbnail = thumbnail;
            Attach();
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public IThumbnail Thumbnail => _thumbnail;

        public ImageSource? ImageSource
        {
            get { return _imageSource; }
            private set
            {
                if (SetProperty(ref _imageSource, value))
                {
                    RaisePropertyChanged(nameof(Width));
                    RaisePropertyChanged(nameof(Height));
                }
            }
        }

        public double Width
        {
            get
            {
                if (_imageSource is null)
                {
                    return 0.0;
                }
                else if (_imageSource is BitmapSource bitmapSource)
                {
                    return bitmapSource.PixelWidth;
                }
                else
                {
                    var aspectRatio = _imageSource.Width / _imageSource.Height;
                    return aspectRatio < 1.0 ? 256.0 * aspectRatio : 256.0;
                }
            }
        }

        public double Height
        {
            get
            {
                if (_imageSource is null)
                {
                    return 0.0;
                }
                else if (_imageSource is BitmapSource bitmapSource)
                {
                    return bitmapSource.PixelHeight;
                }
                else
                {
                    var aspectRatio = _imageSource.Width / _imageSource.Height;
                    return aspectRatio < 1.0 ? 256.0 : 256.0 / aspectRatio;
                }
            }
        }

        public bool IsUniqueImage => _thumbnail?.IsUniqueImage ?? false;
        public bool IsNormalImage => _thumbnail?.IsNormalImage ?? false;
        public Brush Background => _thumbnail?.Background ?? Brushes.Transparent;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Detach();
                }
                _imageSource = null;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Attach()
        {
            UpdateBitmapSourceAsync();
            _thumbnail.Changed += Thumbnail_Changed;
        }

        private void Detach()
        {
            _thumbnail.Changed -= Thumbnail_Changed;
        }

        /// <summary>
        /// Thumbnail変更イベント処理
        /// </summary>
        private void Thumbnail_Changed(object? sender, EventArgs e)
        {
            UpdateBitmapSourceAsync();
        }

        /// <summary>
        /// BitmapSource更新
        /// </summary>
        private async void UpdateBitmapSourceAsync()
        {
            if (_thumbnail is not null)
            {
                // BitmapSource生成 (非同期)
                await Task.Run(() =>
                {
                    var imageSource = _thumbnail.CreateImageSource();
                    AppDispatcher.BeginInvoke(() => ImageSource = imageSource);
                });
            }
        }

    }
}
