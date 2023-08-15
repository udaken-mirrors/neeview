using System;
using System.Windows;

namespace NeeView.PageFrames
{
    public interface IScaleControl
    {
        public double Scale { get; }
        public void SetScale(double value, TimeSpan span);
    }

    public interface IAngleControl
    {
        public double Angle { get; }
        public void SetAngle(double value, TimeSpan span);
    }

    public interface IPointControl
    {
        public Point Point { get; }
        public void SetPoint(Point value, TimeSpan span);
        public void AddPoint(Vector value, TimeSpan span);
    }

    public interface IFlipControl
    {
        public bool IsFlipHorizontal { get; }
        public bool IsFlipVertical { get; }
        public void SetFlipHorizontal(bool value, TimeSpan span);
        public void SetFlipVertical(bool value, TimeSpan span);
    }


    public interface ITransformControl : IScaleControl, IAngleControl, IPointControl, IFlipControl
    {
        void SnapView();
    }
}