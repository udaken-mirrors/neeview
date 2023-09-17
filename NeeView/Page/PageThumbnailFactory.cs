namespace NeeView
{
    public static class PageThumbnailFactory
    {
        public static PageThumbnail Create(PageContent content)
        {
            switch (content)
            {
                case BitmapPageContent bitmapPageContent:
                    return new BitmapPageThumbnail(bitmapPageContent);

                case AnimatedPageContent animatedPageContent:
                    return new AnimatedPageThumbnail(animatedPageContent);

                case PdfPageContent pdfPageContent:
                    return new PdfPageThumbnail(pdfPageContent);

                case SvgPageContent svgPageContent:
                    return new SvgPageThumbnail(svgPageContent);

                case ArchivePageContent archivePageContent:
                    return new ArchivePageThumbnail(archivePageContent);

                case MediaPageContent mediaPageContent:
                    return new MediaPageThumbnail(mediaPageContent);

                default:
                    // not support yet.
                    return new PageThumbnail(content);
            }
        }
    }



}
