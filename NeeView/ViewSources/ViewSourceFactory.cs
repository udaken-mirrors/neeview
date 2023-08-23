using System;

namespace NeeView
{
    public class ViewSourceFactory
    {
        private BookMemoryService _bookMemoryService;

        public ViewSourceFactory(BookMemoryService bookMemoryService)
        {
            _bookMemoryService = bookMemoryService;
        }

        public ViewSource Create(PageContent pageContent)
        {
            switch (pageContent)
            {
                case BitmapPageContent bitmapPageContent:
                    return new BitmapViewSource(bitmapPageContent, _bookMemoryService);
                case AnimatedPageContent animatedPageContent:
                    return new AnimatedViewSource(animatedPageContent, _bookMemoryService);
                case PdfPageContent pdfPageContent:
                    return new PdfViewSource(pdfPageContent, _bookMemoryService);
                case SvgPageContent svgPageContent:
                    return new SvgViewSource(svgPageContent, _bookMemoryService);
                case MediaPageContent mediaPageContent:
                    return new MediaViewSource(mediaPageContent, _bookMemoryService);
                case ArchivePageContent archivePageContent:
                    return new ArchiveViewSource(archivePageContent, _bookMemoryService);
                case FilePageContent archivePageContent:
                    return new FileViewSource(archivePageContent, _bookMemoryService);
                default:
                    throw new NotImplementedException();
            }
        }
    }

}
