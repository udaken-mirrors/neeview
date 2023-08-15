using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView.PageFrames
{
    public class PageFrameViewTransform : IPointControl, INotifyTransformChanged
    {
        private MultiTransform _transform = new MultiTransform();
        private LoupeTransformContext _loupeContext;


        public PageFrameViewTransform(BookContext context, LoupeTransformContext loupeContext)
        {
            _loupeContext = loupeContext;

            TransformView = new TransformGroup();
            TransformView.Children.Add(_transform.TransformView);
            TransformView.Children.Add(_loupeContext.GetCanvasTransform());

            TransformCalc = new TransformGroup();
            TransformCalc.Children.Add(_transform.TransformCalc);
            TransformCalc.Children.Add(_loupeContext.GetCanvasTransform());

            loupeContext.TransformChanged += LoupeContext_TransformChanged;
        }


        public event TransformChangedEventHandler? TransformChanged;


        private void LoupeContext_TransformChanged(object? sender, TransformChangedEventArgs e)
        {
            TransformChanged?.Invoke(sender, e);
        }

        public TransformGroup TransformView { get; private set; }
        public TransformGroup TransformCalc { get; private set; }

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
            TransformChanged?.Invoke(this, new TransformChangedEventArgs(TransformCategory.View, TransformAction.Point));
        }

        public void Flush()
        {
            _transform.Flush();
        }
    }
}
