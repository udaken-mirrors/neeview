using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NeeView.ComponentModel
{
    public static class RectExtensions
    {
        public static bool LessThanZero(this Rect rect)
        {
            return rect.Width <= 0.0 || rect.Height <= 0.0;
        }

        public static Point Center(this Rect rect)
        {
            return new Point(rect.Left + rect.Width * 0.5, rect.Top + rect.Height * 0.5);
        }

        public static double Edge(this Rect rect, PageFrameOrientation orientation, int direction)
        {
            Debug.Assert(direction is -1 or 0 or +1);
            return direction switch
            {
                -1 => rect.EdgeMin(orientation),
                +1 => rect.EdgeMax(orientation),
                _ => rect.EdgeCenter(orientation),
            };
        }

        public static double EdgeMin(this Rect rect, PageFrameOrientation orientation)
        {
            return orientation == PageFrameOrientation.Horizontal ? rect.Left : rect.Top;
        }

        public static double EdgeMax(this Rect rect, PageFrameOrientation orientation)
        {
            return orientation == PageFrameOrientation.Horizontal ? rect.Right : rect.Bottom;
        }

        public static double EdgeCenter(this Rect rect, PageFrameOrientation orientation)
        {
            return (rect.EdgeMin(orientation) + rect.EdgeMax(orientation)) * 0.5;
        }
    }


}
