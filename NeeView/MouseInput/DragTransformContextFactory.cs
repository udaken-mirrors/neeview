using System.ComponentModel;
using System.Windows;
using NeeView.PageFrames;

namespace NeeView
{
    /// <summary>
    /// DragTransform 生成
    /// </summary>
    public class DragTransformContextFactory
    {
        // TODO: PageFrameBoxは範囲が大きい。必要な機能をInterfaceにして渡す。
        private readonly PageFrameBox _box;
        private readonly TransformControlFactory _transformControlFactory;
        private readonly ViewConfig _viewConfig;
        private readonly MouseConfig _mouseConfig;
        private readonly LoupeConfig _loupeConfig;

        public DragTransformContextFactory(PageFrameBox box, TransformControlFactory transformControlFactory, ViewConfig viewConfig, MouseConfig mouseConfig, LoupeConfig loupeConfig)
        {
            _box = box;
            _transformControlFactory = transformControlFactory;
            _viewConfig = viewConfig;
            _mouseConfig = mouseConfig;
            _loupeConfig = loupeConfig;
        }


        public ContentDragTransformContext CreateContentDragTransformContext(PageFrameContainer container)
        {
            var transformControl = _transformControlFactory.Create(container);
            return new ContentDragTransformContext(_box, transformControl, container, _box, _viewConfig, _mouseConfig);
        }

        public LoupeDragTransformContext CreateLoupeDragTransformContext()
        {
            var transformControl = _transformControlFactory.CreateLoupe();
            return new LoupeDragTransformContext(_box, transformControl, _viewConfig, _mouseConfig, _loupeConfig);
        }

#if false
        private Rect CreateViewRect()
        {
            var viewRect = new Size(_box.ActualWidth, _box.ActualHeight).ToRect();
            return viewRect;
        }

        private Rect CreateContentRect(PageFrameContainer container)
        {
            var rect = container.GetContentRect();
            var p0 = _box.TranslateCanvasToViewPoint(container.TranslateContentToCanvasPoint(rect.TopLeft));
            var p1 = _box.TranslateCanvasToViewPoint(container.TranslateContentToCanvasPoint(rect.BottomRight));
            var contentRect = new Rect(p0, p1);
            return contentRect;
        }
#endif
    }
}
