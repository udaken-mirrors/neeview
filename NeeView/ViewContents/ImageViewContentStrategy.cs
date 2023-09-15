using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NeeView.Threading;

namespace NeeView
{
    public class ImageViewContentStrategy : IDisposable, IViewContentStrategy
    {
        private readonly ViewContent _viewContent;
        private readonly InstantDelayAction _delayAction = new();
        private ImageContentControl? _imageControl;
        private bool _disposedValue;
        public BitmapScalingMode? _scalingMode;


        public ImageViewContentStrategy(ViewContent viewContent)
        {
            _viewContent = viewContent;
        }


        public ImageSource? ImageSource => _imageControl?.ImageSource;

        public BitmapScalingMode? ScalingMode
        {
            get { return _scalingMode; }
            set
            {
                if (_scalingMode != value)
                {
                    _scalingMode = value;
                    if (_imageControl != null)
                    {
                        _imageControl.ScalingMode = _scalingMode;
                    }
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _delayAction.Dispose();
                    _imageControl?.Dispose();
                    _imageControl = null;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void OnSourceChanged()
        {
            if (_disposedValue) return;

            _delayAction.Request(
                () => _viewContent.RequestLoadViewSource(CancellationToken.None),
                TimeSpan.FromMilliseconds(200)
            );
        }

        public FrameworkElement CreateLoadedContent(object data)
        {
            var imageSource = data as ImageSource ?? throw new InvalidOperationException();

            _imageControl?.Dispose();
            _imageControl = null;

            _imageControl = new ImageContentControl(_viewContent.Element, imageSource, _viewContent.ViewContentSize, _viewContent.BackgroundSource);
            _imageControl.ScalingMode = ScalingMode;
            return _imageControl;
        }


    }
}
