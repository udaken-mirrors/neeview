using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace NeeView.PageFrames
{
    public class DummyTransformControl : ITransformControl
    {
        public double Scale => 1.0;

        public double Angle => 0.0;

        public Point Point => default;

        public bool IsFlipHorizontal => false;

        public bool IsFlipVertical => false;

        public void SetScale(double value, TimeSpan span, TransformTrigger trigger = TransformTrigger.None)
        {
        }

        public void SetAngle(double value, TimeSpan span)
        {
        }

        public void SetPoint(Point value, TimeSpan span)
        {
        }

        public void SetPoint(Point value, TimeSpan span, IEasingFunction? easeX, IEasingFunction? easeY)
        {
        }

        public void SetFlipHorizontal(bool value, TimeSpan span)
        {
        }

        public void SetFlipVertical(bool value, TimeSpan span)
        {
        }

        public void SnapView()
        {
        }
    }





}
