using NeeView.Collections.Generic;
using System.Windows.Media;


namespace NeeView
{
    /// <summary>
    /// MediaPlayer Pool
    /// </summary>
    public static class MediaPlayerPool
    {
        public static ObjectPool<MediaPlayer> Default { get; } = new();
    }

#if false
    public partial class MediaViewContent : ViewContent, IHasMediaPlayer
    {
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        private readonly IViewContentStrategy _strategy;


        public MediaViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
            _strategy = new MediaViewContentStrategy(this);
        }


        public IMediaPlayer? Player => (_strategy as MediaViewContentStrategy)?.Player;


        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _strategy.Dispose();
                    _disposables.Dispose();
                    this.Content = null;
                }
                _disposedValue = true;
            }

            base.Dispose(disposing);
        }

        protected override void OnSourceChanged()
        {
            if (_disposedValue) return;
            _strategy.OnSourceChanged();
            base.OnSourceChanged();
        }

        protected override FrameworkElement CreateLoadedContent(object data)
        {
            return _strategy.CreateLoadedContent(data);
        }
    }
#endif

}
