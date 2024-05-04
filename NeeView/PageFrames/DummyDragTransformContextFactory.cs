using System.Windows;

namespace NeeView.PageFrames
{
    public class DummyDragTransformContextFactory : IDragTransformContextFactory
    {
        private readonly FrameworkElement _sender;
        private readonly ViewConfig _viewConfig;
        private readonly MouseConfig _mouseConfig;
        private readonly ITransformControl _transform;

        public DummyDragTransformContextFactory(FrameworkElement sender, ViewConfig viewConfig, MouseConfig mouseConfig)
        {
            _sender = sender;
            _viewConfig = viewConfig;
            _mouseConfig = mouseConfig;
            _transform = new DummyTransformControl();
        }

        public ContentDragTransformContext? CreateContentDragTransformContext(bool isPointContainer)
        {
            return new DummyContentDragTransformContext(_sender, _transform, _viewConfig, _mouseConfig);
        }

        public ContentDragTransformContext? CreateContentDragTransformContext(PageFrameContainer container)
        {
            return new DummyContentDragTransformContext(_sender, _transform, _viewConfig, _mouseConfig);
        }

        public LoupeDragTransformContext? CreateLoupeDragTransformContext()
        {
            return null;
        }
    }
}
