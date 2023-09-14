using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Threading;
using System;
using System.Threading;
using System.Windows;

namespace NeeView
{
    public class FileViewContent : ViewContent, IDisposable
    {
        private FilePageControl? _pageControl;
        private bool _disposedValue;
        private DisposableCollection _disposables = new();
        private InstantDelayAction _delayAction;

        public FileViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
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

        protected override FrameworkElement CreateLoadedContent(object data)
        {
            if (_pageControl is not null)
            {
                return _pageControl;
            }

            _pageControl = new FilePageControl((FilePageSource)data);
            return _pageControl;
        }
    }
}
