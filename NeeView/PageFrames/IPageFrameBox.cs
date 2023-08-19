using System;
using System.ComponentModel;
using NeeView.ComponentModel;
using NeeView;

namespace NeeView.PageFrames
{
    // TODO: これは abstract class にすべきだったかな。 Dummy はそれで。
    public interface IPageFrameBox : IDragTransformContextFactory
    {
        void MoveTo(PagePosition position, LinkedListDirection direction);
        void MoveToNextPage(LinkedListDirection direction);
        void MoveToNextFrame(LinkedListDirection direction);
        void MoveToNextFolder(LinkedListDirection direction, bool isShowMessage);
        void ScrollToNextFrame(LinkedListDirection direction, IScrollNTypeParameter parameter, LineBreakStopMode lineBreakStopMode, double endMargin);
        bool ScrollToNext(LinkedListDirection direction, IScrollNTypeParameter parameter);

        PageFrameTransformAccessor? CreateSelectedTransform();
        void ResetTransform();
        void Stretch(bool ignoreViewOrigin);

        void Reset();
    }

    public interface IDragTransformContextFactory
    {
        DragTransformContext? CreateDragTransformContext(bool isPointContainer, bool isLoupeTransform);
    }
}
