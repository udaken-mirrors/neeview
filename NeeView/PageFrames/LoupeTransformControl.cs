using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public class LoupeTransformControl : ITransformControl
    {
        private LoupeTransformContext _loupeContext;


        public LoupeTransformControl(LoupeTransformContext loupeContext)
        {
            _loupeContext = loupeContext;
        }

        public double Scale => _loupeContext.Scale;

        public double Angle => 0.0;
        public Point Point => _loupeContext.Point;
        public bool IsFlipHorizontal => false;
        public bool IsFlipVertical => false;


        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
            // nop.
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
            // nop.
        }

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
            _loupeContext.SetScale(value, span, trigger);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            // nop.
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            SetPoint(value, span, null, null);
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _loupeContext.SetPoint(value, span, easeX, easeY);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            AddPoint(value, span, null, null);
        }

        public void AddPoint(Vector value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
            _loupeContext.AddPoint(value, span, easeX, easeY);
        }

        public void SnapView()
        {
        }

    }
}
