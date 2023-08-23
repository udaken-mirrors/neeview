using NeeView.IO;
using System.IO;
using System.Threading;

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

        private Page CreatePage(string bookPrefix, ArchiveEntry entry, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            return new Page(bookPrefix, CreatePageContent(entry, token));
        }

        public PageContent CreatePageContent(ArchiveEntry entry, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (entry.IsImage())
            {
                if (entry.Archiver is MediaArchiver)
                {
                    return new MediaPageContent(entry, _bookMemoryService);
                }
                else if (entry.Archiver is PdfArchiver)
                {
                    return new PdfPageContent(entry, _bookMemoryService);
                }
                else if (Config.Current.Image.Standard.IsAnimatedGifEnabled && LoosePath.GetExtension(entry.Link ?? entry.EntryName) == ".gif")
                {
                    return new AnimatedPageContent(entry, _bookMemoryService);
                }
                else if (PictureProfile.Current.IsSvgSupported(entry.EntryName))
                {
                    return new SvgPageContent(entry, _bookMemoryService);
                }
                else if (PictureProfile.Current.IsMediaSupported(entry.EntryName))
                {
                    return new MediaPageContent(entry, _bookMemoryService);
                }
                else
                {
                    return new BitmapPageContent(entry, _bookMemoryService);
                }
            }
            else if (entry.IsBook())
            {
                return new ArchivePageContent(entry, _bookMemoryService);
                //page.Thumbnail.IsCacheEnabled = true;
            }
            else
            {
                var type = entry.IsDirectory ? ArchiverType.FolderArchive : ArchiverManager.Current.GetSupportedType(entry.Link ?? entry.EntryName);
                switch (type)
                {
                    case ArchiverType.None:
                        if (Config.Current.Image.Standard.IsAllFileSupported)
                        {
                            entry.IsIgnoreFileExtension = true;
                            return new BitmapPageContent(entry, _bookMemoryService);
                        }
                        else
                        {
                            return new FilePageContent(entry, FilePageIcon.File, null, _bookMemoryService);
                        }
                    case ArchiverType.FolderArchive:
                        return new FilePageContent(entry, FilePageIcon.Folder, null, _bookMemoryService);
                    default:
                        return new FilePageContent(entry, FilePageIcon.Archive, null, _bookMemoryService);
                }
            }
        }
    }




}
