namespace NeeView.PageFrames
{
    public interface IDragTransformContextFactory
    {
        DragTransformContext? CreateDragTransformContext(bool isPointContainer, bool isLoupeTransform);
        DragTransformContext? CreateDragTransformContext(PageFrameContainer container, bool isLoupeTransform);
    }
}
