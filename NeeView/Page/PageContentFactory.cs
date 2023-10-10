using NeeView.IO;
using System.IO;
using System.Threading;

namespace NeeView
{
    public class PageContentFactory
    {
        private readonly BookMemoryService? _bookMemoryService;
        private readonly bool _allowAnimatedImage;

        public PageContentFactory(BookMemoryService? bookMemoryService, bool arrowAnimatedImage)
        {
            _bookMemoryService = bookMemoryService;
            _allowAnimatedImage = arrowAnimatedImage;
        }


        public PageContent CreatePageContent(ArchiveEntry entry, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var name = entry.Link ?? entry.EntryName;
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
                else if (PictureProfile.Current.IsSvgSupported(name))
                {
                    return new SvgPageContent(entry, _bookMemoryService);
                }
                else if (PictureProfile.Current.IsMediaSupported(name))
                {
                    return new MediaPageContent(entry, _bookMemoryService);
                }
                else if (_allowAnimatedImage && PictureProfile.Current.IsAnimatedGifSupported(name))
                {
                    return new AnimatedPageContent(entry, _bookMemoryService, AnimatedImageType.Gif);
                }
                else if (_allowAnimatedImage && PictureProfile.Current.IsAnimatedPngSupported(name))
                {
                    return new AnimatedPageContent(entry, _bookMemoryService, AnimatedImageType.Png);
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
                var type = entry.IsDirectory ? ArchiverType.FolderArchive : ArchiverManager.Current.GetSupportedType(name);
                switch (type)
                {
                    case ArchiverType.None:
                        if (Config.Current.Image.Standard.IsAllFileSupported)
                        {
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
