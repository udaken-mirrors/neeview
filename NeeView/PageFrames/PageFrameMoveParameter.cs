//#define LOCAL_DEBUG

using NeeView.ComponentModel;

namespace NeeView.PageFrames
{
    /// <summary>
    /// ページ移動パラメータ
    /// </summary>
    public class PageFrameMoveParameter
    {
        public PageFrameMoveParameter(PageFrameContainer container, LinkedListDirection direction, bool isContinued, bool isFlush)
            : this(container.Content.FrameRange.Top(direction.ToSign()), direction, isContinued, isFlush)
        {
        }

        public PageFrameMoveParameter(PagePosition position, LinkedListDirection direction, bool isContinued, bool isFlush)
        {
            Position = position;
            Direction = direction;
            IsContinued = isContinued;
            IsFlush = isFlush;
        }

        public PagePosition Position { get; }
        public LinkedListDirection Direction { get; }
        public bool IsContinued { get; }
        public bool IsFlush { get; }
    }
}
