using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Threading;

namespace NeeView
{
    public class ImageViewContent : ViewContent
    {
        private bool _disposedValue;
        private DisposableCollection _disposables = new();
        private InstantDelayAction _delayAction;

        public ImageViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity)
            : base(element, scale, viewSource, activity)
        {
            _delayAction = new InstantDelayAction();
            _disposables.Add(_delayAction);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
            var imageSource = data as ImageSource ?? throw new InvalidOperationException();
            return new ImageContentControl(Element, imageSource, ViewContentSize);
        }

    }
}
