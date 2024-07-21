//#define LOCAL_DEBUG

using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    /// <summary>
    /// ページ移動パラメータ
    /// </summary>
    public class PageFrameMoveParameter
    {
        public PageFrameMoveParameter(PageFrameContainer container, LinkedListDirection direction, bool isPositionFixed, bool isRelational, bool isFlush)
            : this(container.Content.FrameRange.Top(direction.ToSign()), direction, isPositionFixed, isRelational, isFlush)
        {
        }

        public PageFrameMoveParameter(PagePosition position, LinkedListDirection direction, bool isPositionFixed, bool isRelational, bool isFlush)
        {
            Position = position;
            Direction = direction;
            IsPositionFixed = isPositionFixed;
            IsRelational = isRelational;
            IsFlush = isFlush;
        }

        public PagePosition Position { get; }
        public LinkedListDirection Direction { get; }
        public bool IsPositionFixed { get; }
        public bool IsRelational { get; }
        public bool IsFlush { get; }
    }
}
