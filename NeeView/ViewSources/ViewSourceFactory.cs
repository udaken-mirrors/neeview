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

        public ViewSource Create(IPageContent pageContent)
        {
            switch (pageContent)
            {
                case BitmapPageContent bitmapPageContent:
                    return new BitmapViewSource(bitmapPageContent, _bookMemoryService);
                case MediaPageContent mediaPageContent:
                    return new MediaViewSource(mediaPageContent, _bookMemoryService);
                default:
                    throw new NotImplementedException();
            }
        }
    }

}
