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

        public static Rect InflateValid(this Rect rect, double dx, double dy)
        {
            double x;
            double width;
            if (rect.Width + dx * 2.0 >= 0.0)
            {
                x = rect.X - dx;
                width = rect.Width + dx * 2.0;
            }
            else
            {
                x = rect.X + rect.Width * 0.5;
                width = 0.0;
            }

            double y;
            double height;
            if (rect.Height + dy * 2.0 >= 0.0)
            {
                y = rect.Y - dy;
                height = rect.Height + dy * 2.0;
            }
            else
            {
                y = rect.Y + rect.Height * 0.5;
                height = 0.0;
            }

            return new Rect(x, y, width, height);
        }

    }


}
