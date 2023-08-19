using System.IO;

namespace NeeView
{
    public class PageContentFactory
    {
        private BookMemoryService _bookMemoryService;

        public PageContentFactory(BookMemoryService bookMemoryService)
        {
            _bookMemoryService = bookMemoryService;
        }

        public PageContent Create(ArchiveEntry archiveEntry)
        {
            var ext = Path.GetExtension(archiveEntry.EntryName).ToLower();

            switch (ext)
            {
                case ".mp4":
                case ".gif":
                case ".mkv":
                    return new MediaPageContent(archiveEntry, _bookMemoryService);
                default:
                    return new BitmapPageContent(archiveEntry, _bookMemoryService);
            }

        }
    }




}
