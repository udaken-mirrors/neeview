using System;
using System.Windows;

namespace NeeView.PageFrames
{
    public class LoupeTransformControl : ITransformControl
    {
        private PageFrameContainer _container;
        private LoupeTransformContext _loupeContext;


        public LoupeTransformControl(PageFrameContainer container, LoupeTransformContext loupeContext)
        {
            _container = container;
            _loupeContext = loupeContext;
        }

        public double Scale => _container.Transform.Scale;
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

        public void SetScale(double value, TimeSpan span)
        {
            _loupeContext.SetScale(value, span);
        }

        public void SetAngle(double value, TimeSpan span)
        {
            // nop.
        }

        public void SetPoint(Point value, TimeSpan span)
        {
            _loupeContext.SetPoint(value, span);
        }

        public void AddPoint(Vector value, TimeSpan span)
        {
            _loupeContext.AddPoint(value, span);
        }

        public void SnapView()
        {
        }

    }
}