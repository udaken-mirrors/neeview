using NeeView.PageFrames;
using System;

namespace NeeView
{
    public class ViewContentFactory
    {
        //private ViewContentParameters _parameter;
        private ViewSourceMap _viewSourceMap;

        public ViewContentFactory(ViewSourceMap viewSourceMap)
        {
            _viewSourceMap = viewSourceMap;

            // TODO: 本来はここで環境パラメータ生成
            // TODO: ViewContentParameters は不要では？
            //_parameter = new ViewContentParameters();
        }

        public ViewContent Create(PageFrameElement element, PageFrameElementScale scale, PageFrameActivity activity)
        {
            var viewSource = _viewSourceMap.Get(element.PageRange, element.Page.Content);

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
