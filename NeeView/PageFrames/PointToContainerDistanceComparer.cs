using System;
using System.Collections.Generic;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 点とコンテナの距離比較
    /// </summary>
    public class PointToContainerDistanceComparer : IComparer<PageFrameContainer>
    {
        private Point _point;
        private readonly Func<PageFrameContainer, PageFrameContainer, int> _compare;

        public PointToContainerDistanceComparer(PageFrameOrientation orientation, Point point)
        {
            _point = point;
            _compare = orientation == PageFrameOrientation.Horizontal ? HorizontalCompare : VerticalCompare;
        }

        public int Compare(PageFrameContainer? c0, PageFrameContainer? c1)
        {
            if (c0 is null)
            {
                return c1 is null ? 0 : -1;
            }
            else if (c1 is null)
            {
                return 1;
            }

            return _compare(c0, c1);
        }

        private int HorizontalCompare(PageFrameContainer c0, PageFrameContainer c1)
        {
            var x = _point.X;
            var isInside0 = IsInsideOfHorizonLine(c0, x);
            var isInside1 = IsInsideOfHorizonLine(c1, x);

            if (isInside0)
            {
                // NOTE: 小数点以下は誤差とする
                return isInside1 ? (int)(GetHorizontalInsideDistance(c0, x) - GetHorizontalInsideDistance(c1, x)) : -1;
            }
            else
            {
                return isInside1 ? 1 : (int)(GetHorizontalOutsideDistance(c0, x) - GetHorizontalOutsideDistance(c1, x));
            }
        }

        private int VerticalCompare(PageFrameContainer c0, PageFrameContainer c1)
        {
            var y = _point.Y;
            var isInside0 = IsInsideOfVerticalLine(c0, y);
            var isInside1 = IsInsideOfVerticalLine(c1, y);

            if (isInside0)
            {
                // NOTE: 小数点以下は誤差とする
                return isInside1 ? (int)(GetVerticalInsideDistance(c0, y) - GetVerticalInsideDistance(c1, y)) : -1;
            }
            else
            {
                return isInside1 ? 1 : (int)(GetVerticalOutsideDistance(c0, y) - GetVerticalOutsideDistance(c1, y));
            }
        }

        private static bool IsInsideOfHorizonLine(PageFrameContainer c, double x)
        {
            return c.X < x && x < c.X + c.Width;
        }

        private static double GetHorizontalInsideDistance(PageFrameContainer c, double x)
        {
            return Math.Abs(c.X + c.Width * 0.5 - x);
        }

        private static double GetHorizontalOutsideDistance(PageFrameContainer c, double x)
        {
            return Math.Min(Math.Abs(c.X - x), Math.Abs(c.X + c.Width - x));
        }

        private static bool IsInsideOfVerticalLine(PageFrameContainer c, double y)
        {
            return c.Y < y && y < c.Y + c.Height;
        }

        private static double GetVerticalInsideDistance(PageFrameContainer c, double y)
        {
            return Math.Abs(c.Y + c.Height * 0.5 - y);
        }

        private static double GetVerticalOutsideDistance(PageFrameContainer c, double y)
        {
            return Math.Min(Math.Abs(c.Y - y), Math.Abs(c.Y + c.Height - y));
        }
    }

}
