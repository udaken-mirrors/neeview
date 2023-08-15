using System.Windows;
using NeeView.PageFrames;

namespace NeeView
{
    public class LoupeDragTransformContext : DragTransformContext
    {
        private LoupeConfig _loupeConfig;

        public LoupeDragTransformContext(FrameworkElement sender, ITransformControl transform, Rect contentRect, Rect viewRect, ViewConfig viewConfig, LoupeConfig loupeConfig)
            : base(sender, transform, contentRect, viewRect, viewConfig)
        {
            _loupeConfig = loupeConfig;
        }


        public Point LoupeBasePoint { get; set; }
        public double LoupeSpeed => _loupeConfig.Speed;
        public LoupeConfig Loupe => _loupeConfig;

        public double LoupeScale
        {
            get { return _loupeConfig.LoupeScale; }
            set { _loupeConfig.LoupeScale = value; }
        }

        public override void Initialize(Point point, int timestamp)
        {
            base.Initialize(point, timestamp);

            var center = new Point(0, 0); // ViewRect.Center();
            Vector v = First - center;
            LoupeBasePoint = (Point)(_loupeConfig.IsLoupeCenter ? -v : -v + v / LoupeScale);
        }
    }



}