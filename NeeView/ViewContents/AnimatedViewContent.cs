using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using System;
using System.Windows.Media;
using System.Windows;

namespace NeeView
{
    public class AnimatedViewContent : ViewContent, IHasImageSource, IHasScalingMode, IHasMediaPlayer
    {
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private IViewContentStrategy? _strategy;

        public AnimatedViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
        }


        public IMediaPlayer? Player => (_strategy as MediaViewContentStrategy)?.Player;

        public ImageSource? ImageSource => _strategy?.ImageSource;

        public BitmapScalingMode? ScalingMode
        {
            get { return _strategy?.ScalingMode; }
            set { if (_strategy is not null) _strategy.ScalingMode = value; }
        }


        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _strategy?.Dispose();
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
            _strategy?.OnSourceChanged();
            base.OnSourceChanged();
        }

        protected override FrameworkElement CreateLoadedContent(object data)
        {
            _strategy = _strategy ?? CreateStrategy(data);
            return _strategy.CreateLoadedContent(data);
        }

        private IViewContentStrategy CreateStrategy(object data)
        {
            return data switch
            {
                ImageSource _ => new ImageViewContentStrategy(this),
                MediaSource _ => new MediaViewContentStrategy(this),
                _ => throw new NotSupportedException(),
            };
        }
    }

}
