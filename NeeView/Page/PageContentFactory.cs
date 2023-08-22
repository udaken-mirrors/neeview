using System.IO;

namespace NeeView
{
    public class PageContentFactory
    {
        private BookMemoryService? _bookMemoryService;

        public PageContentFactory(BookMemoryService? bookMemoryService)
        {
            _bookMemoryService = bookMemoryService;
        }

        public PageContent Create(ArchiveEntry archiveEntry)
        {
            var ext = Path.GetExtension(archiveEntry.EntryName).ToLower();

            if (archiveEntry.Archiver is PdfArchiver)
            {
                return new PdfPageContent(archiveEntry, _bookMemoryService);
            }

            switch (ext)
            {
                case ".svg":
                    return new SvgPageContent(archiveEntry, _bookMemoryService);
                case ".gif":
                    return new AnimatedPageContent(archiveEntry, _bookMemoryService);
                case ".mp4":
                case ".mkv":
                    return new MediaPageContent(archiveEntry, _bookMemoryService);
                default:
                    return new BitmapPageContent(archiveEntry, _bookMemoryService);
            }

        }
    }




}
