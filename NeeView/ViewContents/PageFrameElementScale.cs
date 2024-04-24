using System.Windows;

namespace NeeView
{
    public record class PageFrameElementScale
    {
        public PageFrameElementScale(double layoutScale, double renderScale, double renderAngle, double baseScale, DpiScale dpiScale)
        {
            LayoutScale = layoutScale;
            RenderScale = renderScale;
            RenderAngle = renderAngle;
            BaseScale = baseScale;
            DpiScale = dpiScale;
        }

        public double LayoutScale { get; }
        public double RenderScale { get; }
        public double RenderAngle { get; }
        public double BaseScale { get; }
        public DpiScale DpiScale { get; }
    }
}
