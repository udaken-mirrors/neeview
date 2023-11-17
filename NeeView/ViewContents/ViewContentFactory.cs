using NeeView.PageFrames;
using System;

namespace NeeView
{
    public class ViewContentFactory
    {
        private readonly ViewSourceMap _viewSourceMap;
        private readonly PageBackgroundSource _backgroundSource;

        public ViewContentFactory(ViewSourceMap viewSourceMap)
        {
            _viewSourceMap = viewSourceMap;
            _backgroundSource = new PageBackgroundSource();
        }

        public ViewContent Create(PageFrameElement element, PageFrameElementScale scale, PageFrameActivity activity, int index)
        {
            var viewSource = _viewSourceMap.Get(element.Page, element.PagePart, element.PageDataSource);
            return new ViewContent(element, scale, viewSource, activity, _backgroundSource, index);
        }
    }
}
