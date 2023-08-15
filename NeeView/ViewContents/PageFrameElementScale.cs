using System.Windows;

namespace NeeView
{
    public record class PageFrameElementScale
    {
        public PageFrameElementScale(double layoutScale, double renderScale, double renderAngle, DpiScale dpiScale)
        {
            LayoutScale = layoutScale;
            RenderScale = renderScale;
            RenderAngle = renderAngle;
            DpiScale = dpiScale;
        }

        public double LayoutScale { get; }
        public double RenderScale { get; }
        public double RenderAngle { get; }
        public DpiScale DpiScale { get; }
    }
}
