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
        private readonly LoupeConfig _loupeConfig;

        public DragTransformContextFactory(PageFrameBox box, TransformControlFactory transformControlFactory, ViewConfig viewConfig, LoupeConfig loupeConfig)
        {
            _box = box;
            _transformControlFactory = transformControlFactory;
            _viewConfig = viewConfig;
            _loupeConfig = loupeConfig;
        }

        public DragTransformContext Create(PageFrameContainer container, bool isLoupe)
        {
            if (isLoupe)
            {
                return CreateLoupe(container);
            }
            else
            {
                return CreateNormal(container);
            }
        }

        private DragTransformContext CreateNormal(PageFrameContainer container)
        {
            var transformControl = _transformControlFactory.Create(container);
            return new DragTransformContext(_box, transformControl, container, _box, _viewConfig);
        }

        private DragTransformContext CreateLoupe(PageFrameContainer container)
        {
            var transformControl = _transformControlFactory.CreateLoupe(container);
            return new LoupeDragTransformContext(_box, transformControl, container, _box, _viewConfig, _loupeConfig);
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