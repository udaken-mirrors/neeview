using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView.PageFrames
{
    public partial class PageFrameViewTransform : IPointControl, INotifyTransformChanged, IDisposable
    {
        private MultiTransform _transform = new MultiTransform();
        private LoupeTransformContext _loupeContext;
        private bool _disposedValue;
        private DisposableCollection _disposables = new();

        public PageFrameViewTransform(BookContext context, LoupeTransformContext loupeContext)
        {
            _loupeContext = loupeContext;
            _disposables.Add(_loupeContext.SubscribeTransformChanged(LoupeContext_TransformChanged));
            _disposables.Add(context.SubscribePropertyChanged(nameof(context.PageMode), (s, e) => Update()));

            Update();
        }


        [Subscribable]
        public event TransformChangedEventHandler? TransformChanged;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Update()
        {
            TransformView.Children.Clear();
            TransformView.Children.Add(_transform.TransformView);
            TransformView.Children.Add(_loupeContext.GetCanvasTransform());

            TransformCalc.Children.Clear();
            TransformCalc.Children.Add(_transform.TransformCalc);
            TransformCalc.Children.Add(_loupeContext.GetCanvasTransform());
        }

        private void LoupeContext_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            TransformChanged?.Invoke(sender, e);
        }

        public TransformGroup TransformView { get; private set; } = new();
        public TransformGroup TransformCalc { get; private set; } = new();

        public Point Point => _transform.Point;


        public TransformGroup GetTransform(TransformSelect select)
        {
            return select == TransformSelect.View ? TransformView : TransformCalc;
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            SetPoint(_transform.Point + value, span);
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            if (_transform.Point == value) return;

            //Debug.WriteLine($"$$ {{{Point:f0}}} to {{{value:f0}}} ({span.TotalMilliseconds})");
            _transform.SetPoint(value, span);
            TransformChanged?.Invoke(this, new TransformChangedEventArgs(this, TransformCategory.View, TransformAction.Point));
        }

        public void Flush()
        {
            _transform.Flush();
        }

    }
}
