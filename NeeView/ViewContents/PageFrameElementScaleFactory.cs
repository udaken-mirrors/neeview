using System.Windows;
using System.Windows.Controls;
using NeeView.PageFrames;

namespace NeeView
{
    public class PageFrameElementScaleFactory
    {
        private PageFrameContext _bookContext;
        private PageFrameTransformMap _transformMap;
        private LoupeTransformContext _loupeTransform;

        public PageFrameElementScaleFactory(PageFrameContext bookContext, PageFrameTransformMap transformMap, LoupeTransformContext loupeTransform)
        {
            _bookContext = bookContext;
            _transformMap = transformMap;
            _loupeTransform = loupeTransform;
        }

        public PageFrameElementScale Create(PageFrame pageFrame)
        {
            var key = PageFrameTransformTool.CreateKey(pageFrame);
            var transformScale = _transformMap.GetScale(key);
            var transformAngle = _transformMap.GetAngle(key);

            return new PageFrameElementScale(
                layoutScale: pageFrame.Scale,
                renderScale: transformScale * _loupeTransform.Scale,
                renderAngle: transformAngle,
                dpiScale: _bookContext.DpiScale);
        }

        public PageFrameElementScale Create(PageFrame pageFrame, IPageFrameTransform transform)
        {
            return new PageFrameElementScale(
                layoutScale: pageFrame.Scale,
                renderScale: transform.Scale * _loupeTransform.Scale,
                renderAngle: transform.Angle,
                dpiScale: _bookContext.DpiScale);
        }

        public static PageFrameElementScale Create(PageFrame pageFrame, IPageFrameTransform transform, LoupeTransformContext loupeTransform, DpiScale dpiScale)
        {
            return new PageFrameElementScale(
                layoutScale: pageFrame.Scale,
                renderScale: transform.Scale * loupeTransform.Scale,
                renderAngle: transform.Angle,
                dpiScale: dpiScale);
        }
    }


}
