using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows;

namespace NeeView
{
    public class ArchivePageContent : PageContent
    {
        private PageContent? _selectedContent;
        private ThumbnailType _thumbnailType;


        public ArchivePageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
        }

        public class ArchiveThumbnailSource
        {
            public ArchiveThumbnailSource(ThumbnailType thumbnailType, PageContent pageContent)
            {
                ThumbnailType = thumbnailType;
                PageContent = pageContent;
            }

            public ThumbnailType ThumbnailType { get; }
            public PageContent PageContent { get; }
        }


        public async Task<ArchiveThumbnailSource> GetArchiveThumbnailSourceAsync(CancellationToken token)
        {
            if (_selectedContent is null)
            {
                var entry = await CreateRegularEntryAsync(Entry, token);
                var selected = await SelectAlternativeEntry(entry, token);
                _thumbnailType = GetThumbnailType(selected);
                var factory = new PageContentFactory(BookMemoryService);
                _selectedContent = factory.Create(selected);
            }

            return new ArchiveThumbnailSource(_thumbnailType, _selectedContent);
        }

        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            try
            {
                var thumbnail = new ArchivePageThumbnail(this);
                await thumbnail.LoadAsync(token);
                return new PageSource(thumbnail.Thumbnail, null, new PictureInfo(new Size(256, 256))); // TODO: size大丈夫？
            }
            catch(OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return PageSource.CreateError(ex.Message);
            }
        }

        private ThumbnailType GetThumbnailType(ArchiveEntry entry)
        {
            if (System.IO.Directory.Exists(entry.SystemPath) || entry.IsBook())
            {
                if (ArchiverManager.Current.GetSupportedType(entry.SystemPath) == ArchiverType.MediaArchiver)
                {
                    return ThumbnailType.Media;
                }
            }

            return ThumbnailType.Unique;
        }

        private async Task<ArchiveEntry> SelectAlternativeEntry(ArchiveEntry entry, CancellationToken token)
        {
            if (System.IO.Directory.Exists(entry.SystemPath) || entry.IsBook())
            {
                if (ArchiverManager.Current.GetSupportedType(entry.SystemPath) == ArchiverType.MediaArchiver)
                {
                    return entry;
                }

                return await ArchiveEntryUtility.CreateFirstImageArchiveEntryAsync(entry, 2, token) ?? entry;
            }
            else
            {
                return entry;
            }
        }

        // 簡易 ArchiveEntry を 正規 ArchiveEntry に変換
        private async Task<ArchiveEntry> CreateRegularEntryAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (!entry.IsTemporary) return entry;

            var query = new QueryPath(entry.SystemPath);
            query = query.ToEntityPath();
            try
            {
                return await ArchiveEntryUtility.CreateAsync(query.SimplePath, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ArchiveContent.Entry: {ex.Message}");
                return entry;
                //return StaticFolderArchive.Default.CreateArchiveEntry(query.SimplePath, true);
            }
        }




    }

}
