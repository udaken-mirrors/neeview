using System.Windows;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;

namespace NeeView
{
    public interface IHasImageSource
    {
        ImageSource? ImageSource { get; }
    }

    public interface IHasScalingMode
    {
        BitmapScalingMode? ScalingMode { get; set; }

    }


    public class ImageViewContent : ViewContent, IHasImageSource, IHasScalingMode
    {
        private bool _disposedValue;
        private DisposableCollection _disposables = new();
        private IViewContentStrategy _strategy;

        public ImageViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
            _strategy = new ImageViewContentStrategy(this);
        }

        public ImageSource? ImageSource => _strategy.ImageSource;

        public BitmapScalingMode? ScalingMode
        {
            get => _strategy.ScalingMode;
            set => _strategy.ScalingMode = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _strategy.Dispose();
                    _disposables.Dispose();
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
}
