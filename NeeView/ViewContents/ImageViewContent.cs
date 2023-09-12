using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Threading;

namespace NeeView
{
    public interface IHasImageSource
    {
        ImageSource? ImageSource { get; }
    }


    public class ImageViewContent : ViewContent, IHasImageSource
    {
        private ImageContentControl? _imageControl;
        private bool _disposedValue;
        private DisposableCollection _disposables = new();
        private InstantDelayAction _delayAction;
        private BitmapScalingMode? _scalingMode;
        private PageBackgroundSource _backgroundSource;


        public ImageViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource)
            : base(element, scale, viewSource, activity, backgroundSource)
        {
            _backgroundSource = backgroundSource;

            _delayAction = new InstantDelayAction();
            _disposables.Add(_delayAction);
        }


        public ImageSource? ImageSource => _imageControl?.ImageSource;

        /// <summary>
        /// BitmapScaleMode指定。Printerで使用される。
        /// </summary>
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



        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _imageControl?.Dispose();
                    _imageControl = null;
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        protected override void OnSourceChanged()
        {
            if (_disposedValue) return;

            _delayAction.Request(
                () => RequestLoadViewSource(CancellationToken.None),
                TimeSpan.FromMilliseconds(200)
            );

            base.OnSourceChanged();
        }



        protected override FrameworkElement CreateLoadedContent(object data)
        {
            _imageControl?.Dispose();
            _imageControl = null;

            var imageSource = data as ImageSource ?? throw new InvalidOperationException();
            _imageControl = new ImageContentControl(Element, imageSource, ViewContentSize, _backgroundSource);
            _imageControl.ScalingMode = _scalingMode;
            return _imageControl;
        }

    }
}
