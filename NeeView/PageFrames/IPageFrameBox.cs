using System;
using System.ComponentModel;
using NeeView.ComponentModel;
using NeeView;

namespace NeeView.PageFrames
{
    // TODO: これは abstract class にすべきだったかな。 Dummy はそれで。
    public interface IPageFrameBox : IDragTransformContextFactory
    {
        public void MoveTo(PagePosition position, LinkedListDirection direction);
        public void MoveToNextPage(LinkedListDirection direction);
        public void MoveToNextFrame(LinkedListDirection direction);
        public void ScrollToNextFrame(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin);
        public bool ScrollToNext(LinkedListDirection direction, IScrollNTypeParameter parameter);
        
        public void ResetTransform();
        public void Stretch(bool ignoreViewOrigin);

        public void Reset();
    }

    public interface IDragTransformContextFactory
    {
        public DragTransformContext? CreateDragTransformContext(bool isPointContainer, bool isLoupeTransform);
    }
}
