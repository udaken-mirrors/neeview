using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Threading;

namespace NeeView
{
    /// <summary>
    /// Bitmap ViewContent
    /// </summary>
    public class BitmapViewContent : ViewContent
    {
        private ConditionalDelayAction _conditionalAction;
        private bool _disposedValue;
        private DisposableCollection _disposables = new();

        private InstantDelayAction _delayAction;


        public BitmapViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity)
            : base(element, scale, viewSource, activity)
        {
            _conditionalAction = new ConditionalDelayAction();
            _disposables.Add(_conditionalAction);

            _delayAction = new InstantDelayAction();
            _disposables.Add(_delayAction);
        }


        public override void Initialize()
        {
            base.Initialize();
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
            var bitmapSource = data as BitmapSource ?? throw new InvalidOperationException();
            return new BitmapViewImage(Element, bitmapSource, ViewContentSize);
        }

    }
}
