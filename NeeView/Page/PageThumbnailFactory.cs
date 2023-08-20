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

                default:
                    // not support yet.
                    return new PageThumbnail(content);
            }
        }
    }



}
