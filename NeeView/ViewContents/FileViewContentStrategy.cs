using NeeLaboratory.ComponentModel;
using NeeView.Threading;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FileViewContentStrategy : IDisposable, IViewContentStrategy
    {
        private readonly ViewContent _viewContent;
        private FilePageControl? _pageControl;
        private readonly DelayAction _delayAction;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        public FileViewContentStrategy(ViewContent viewContent)
        {
            _viewContent = viewContent;

            _delayAction = new DelayAction();
            _disposables.Add(_delayAction);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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
            if (_pageControl is not null)
            {
                return _pageControl;
            }

            _pageControl = new FilePageControl((FileViewData)data);
            return _pageControl;
        }

    }
}
