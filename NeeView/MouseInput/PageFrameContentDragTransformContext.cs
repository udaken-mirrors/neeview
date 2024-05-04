using System;
using System.Windows;
using NeeView.ComponentModel;
using NeeView.PageFrames;

namespace NeeView
{
    /// <summary>
    /// 表示座標系操作のリソース : ページフレーム用
    /// </summary>
    public class PageFrameContentDragTransformContext : ContentDragTransformContext
    {
        private PageFrameContainer _container;
        private readonly ICanvasToViewTranslator _canvasToViewTranslator;


        public PageFrameContentDragTransformContext(FrameworkElement sender, ITransformControl transform, PageFrameContainer container, ICanvasToViewTranslator canvasToViewTranslator, ViewConfig viewConfig, MouseConfig mouseConfig)
            : base(sender, transform, viewConfig, mouseConfig)
        {
            _container = container;
            _canvasToViewTranslator = canvasToViewTranslator;
            ContentRect = CreateContentRect(_container);
        }


        public override void Initialize(Point point, int timestamp)
        {
            base.Initialize(point, timestamp);

            RotateCenter = GetCenterPosition(ViewConfig.RotateCenter);
            ScaleCenter = GetCenterPosition(ViewConfig.ScaleCenter);
            FlipCenter = GetCenterPosition(ViewConfig.FlipCenter);
        }

        private Point GetCenterPosition(DragControlCenter dragControlCenter)
        {
            return dragControlCenter switch
            {
                DragControlCenter.View => ViewRect.Center(), // NOTE: 常に(0,0)
                DragControlCenter.Target => ContentRect.Center(),
                DragControlCenter.Cursor => First,
                _ => throw new NotImplementedException(),
            };
        }

        public override PageFrameContent? GetPageFrameContent()
        {
            return _container.Content as PageFrameContent;
        }

        public override void UpdateRect()
        {
            base.UpdateRect();

            UpdateViewRect();
            ContentRect = CreateContentRect(_container);
        }

        private Rect CreateContentRect(PageFrameContainer container)
        {
            var rect = container.GetContentRect();
            var p0 = _canvasToViewTranslator.TranslateCanvasToViewPoint(container.TranslateContentToCanvasPoint(rect.TopLeft));
            var p1 = _canvasToViewTranslator.TranslateCanvasToViewPoint(container.TranslateContentToCanvasPoint(rect.BottomRight));
            var contentRect = new Rect(p0, p1);
            return contentRect;
        }
    }

}
