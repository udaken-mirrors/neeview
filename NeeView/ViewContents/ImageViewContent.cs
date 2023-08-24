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

        public ImageViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity)
            : base(element, scale, viewSource, activity)
        {
            _delayAction = new InstantDelayAction();
            _disposables.Add(_delayAction);
        }


        public ImageSource? ImageSource => _imageControl?.ImageSource;


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



        protected override FrameworkElement CreateLoadedContent(Size size, object data)
        {
            _imageControl?.Dispose();
            _imageControl = null;

            var imageSource = data as ImageSource ?? throw new InvalidOperationException();
            _imageControl = new ImageContentControl(Element, imageSource, ViewContentSize);
            return _imageControl;
        }

    }
}
