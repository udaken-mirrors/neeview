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
            var viewSource = _viewSourceMap.Get(element.Page, element.PagePart);

            if (element.IsDummy)
            {
                return new DummyViewContent(element, scale, viewSource, activity, _backgroundSource, index);
            }

            switch (element.Page.Content)
            {
                case BitmapPageContent:
                    return new BitmapViewContent(element, scale, viewSource, activity, _backgroundSource, index);
                case AnimatedPageContent:
                    return new AnimatedViewContent(element, scale, viewSource, activity, _backgroundSource, index);
                case PdfPageContent:
                    return new PdfViewContent(element, scale, viewSource, activity, _backgroundSource, index);
                case SvgPageContent:
                    return new SvgViewContent(element, scale, viewSource, activity, _backgroundSource, index);
                case MediaPageContent:
                    return new MediaViewContent(element, scale, viewSource, activity, _backgroundSource, index);
                case ArchivePageContent:
                    return new ArchiveViewContent(element, scale, viewSource, activity, _backgroundSource, index);
                case FilePageContent:
                    return new FileViewContent(element, scale, viewSource, activity, _backgroundSource, index);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
