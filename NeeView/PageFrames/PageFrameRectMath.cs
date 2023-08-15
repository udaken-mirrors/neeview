using System.Diagnostics;
using System.Windows;

namespace NeeView.PageFrames
{
    /// <summary>
    /// 矩形の関係を Orientation, ReadOrder に依存しない正規化した値で計算する
    /// </summary>
    public class PageFrameRectMath
    {
        private BookContext _context;


        public PageFrameRectMath(BookContext context)
        {
            _context = context;
        }


        public PageReadOrder ReadOrder => _context.ReadOrder;
        public PageFrameOrientation FrameOrientation => _context.FrameOrientation;


        public double GetMin(Rect rect)
        {
            return FrameOrientation == PageFrameOrientation.Horizontal
                ? ReadOrder == PageReadOrder.LeftToRight ? rect.Left : -rect.Right
                : rect.Top;
        }

        public double GetMax(Rect rect)
        {
            return FrameOrientation == PageFrameOrientation.Horizontal
                ? ReadOrder == PageReadOrder.LeftToRight ? rect.Right : -rect.Left
                : rect.Bottom;
        }

        public double GetCenter(Rect rect)
        {
            return (GetMin(rect) + GetMax(rect)) * 0.5;
        }

        public bool IsMinOver(Rect rect, Rect rectTarget)
        {
            return GetMin(rect) < GetMin(rectTarget);
        }

        public bool IsMaxOver(Rect rect, Rect rectTarget)
        {
            return GetMax(rectTarget) < GetMax(rect);
        }

        public double GetWidth(Rect rect)
        {
            return FrameOrientation == PageFrameOrientation.Horizontal ? rect.Width : rect.Height;
        }

        public bool IsWidthOver(Rect rect, Rect rectTarget)
        {
            return GetWidth(rectTarget) < GetWidth(rect);
        }

        public Rect GetMarginRect(Rect rect, double margin)
        {
            return FrameOrientation == PageFrameOrientation.Horizontal
                ? new Rect(rect.X - margin, rect.Y, rect.Width + margin * 2.0, rect.Height)
                : new Rect(rect.X, rect.Y - margin, rect.Width, rect.Height + margin * 2.0);
        }

        public PageFrameRectConfrict GetConfrict(Rect rect, Rect rectTarget)
        {
            return new PageFrameRectConfrict(GetConfrictRate(GetMin(rect), rectTarget), GetConfrictRate(GetMax(rect), rectTarget), GetWidth(rectTarget));
        }

        public double GetConfrictRate(double x, Rect rect)
        {
            var min = GetMin(rect);
            var max = GetMax(rect);
            if (min >= max) return 0.0;

            var rate = (x - min) / (max - min);
            return rate;
        }
    }



    public struct PageFrameRectConfrict
    {
        public PageFrameRectConfrict(double minRate, double maxRate, double width)
        {
            MinRate = minRate;
            MaxRate = maxRate;
            Width = width;
        }

        public double MinRate { get; }
        public double MaxRate { get; }
        public double CenterRate => (MinRate + MaxRate) * 0.5;
        public double Width { get; }

        public int Min => RateToState(MinRate);
        public int Max => RateToState(MaxRate);


        public bool IsConfrict()
        {
            return Min == 0 || Max == 0 || IsFilled();
        }

        public bool IsInclued()
        {
            return Min == 0 && Max == 0;
        }

        public bool IsFilled()
        {
            return Min < 0 && 0 < Max;
        }

        public bool IsCentered()
        {
            return MinRate < 0.5 && 0.5 < MaxRate;
        }

        private static int RateToState(double rate)
        {
            return rate < 0.0 ? -1 : rate > 1.0 ? 1 : 0;
        }

        public double GetRate(int direction)
        {
            Debug.Assert(direction is -1 or +1);
            return direction == -1 ? MinRate : MaxRate;
        }

        public int GetState(int direction)
        {
            Debug.Assert(direction is -1 or +1);
            return direction == -1 ? Min : Max;
        }

        public double GetDistance(int direction)
        {
            Debug.Assert(direction is -1 or +1);
            return direction == -1 ? MinDistance() : MaxDistance();
        }

        public double MinDistance()
        {
            return MinRate * Width;
        }

        public double MaxDistance()
        {
            return (1.0 - MaxRate) * Width;
        }

        public bool IsOver(int direction)
        {
            Debug.Assert(direction is -1 or +1);
            return direction == -1 ? IsMinOver() : IsMaxOver();
        }

        public bool IsMinOver()
        {
            return Min < 0 && Max < 0;
        }

        public bool IsMaxOver()
        {
            return 0 < Min && 0 < Max;
        }


        // TODO: direction は 左右方向を入れ替える ... 他のメソッドと相違しているので再検討
        public double GetMinRate(int direction)
        {
            Debug.Assert(direction is -1 or +1);
            return direction == -1 ? 1.0 - MaxRate : MinRate;
        }

        public double GetMaxRate(int direction)
        {
            Debug.Assert(direction is -1 or +1);
            return direction == -1 ? 1.0 - MinRate : MaxRate;
        }
    }


}
