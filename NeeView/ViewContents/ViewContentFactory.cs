using NeeView.PageFrames;
using System;

namespace NeeView
{
    public class ViewContentFactory
    {
        private ViewSourceMap _viewSourceMap;

        public ViewContentFactory(ViewSourceMap viewSourceMap)
        {
            _viewSourceMap = viewSourceMap;
        }

        public ViewContent Create(PageFrameElement element, PageFrameElementScale scale, PageFrameActivity activity)
        {
            var viewSource = _viewSourceMap.Get(element.Page, element.PagePart);

            switch (element.Page.Content)
            {
                case BitmapPageContent:
                    return new BitmapViewContent(element, scale, viewSource, activity);
                case MediaPageContent:
                    return new MediaViewContent(element, scale, viewSource, activity);

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
