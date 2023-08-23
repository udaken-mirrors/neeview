using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using Esprima;

namespace NeeView
{
    public class ArchivePageContent : PageContent
    {
        private PageContent? _selectedContent;
        private ThumbnailType _thumbnailType;

        public ArchivePageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
            Thumbnail.IsCacheEnabled = true;
        }

        public Thumbnail Thumbnail { get; } = new Thumbnail();


        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            try
            {
                //var width = Math.Max(Config.Current.Book.BookPageSize + 100, DefaultSize.Width);
                //var height = Math.Max(Config.Current.Book.BookPageSize + 100, DefaultSize.Height);
                //var pictureInfo = new PictureInfo(new Size(width, height));
                var pictureInfo = new PictureInfo(DefaultSize);

                NVDebug.AssertMTA();
                if (Thumbnail.IsValid) return new PageSource(Thumbnail, null, pictureInfo);

                await Thumbnail.InitializeAsync(Entry, null, token);
                if (Thumbnail.IsValid) return new PageSource(Thumbnail, null, pictureInfo);

                var source = await LoadThumbnailAsync(token);
                Thumbnail.Initialize(source);
                return new PageSource(Thumbnail, null, pictureInfo);
            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return PageSource.CreateError(ex.Message);
            }
        }


        private async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NVDebug.AssertMTA();

            var source = await GetArchiveThumbnailSourceAsync(token);
            if (source.ThumbnailType == ThumbnailType.Unique)
            {
                if (source.PageContent is not ArchivePageContent)
                {
                    Debug.Assert(source.PageContent is not ArchivePageContent);
                    var pageThumbnail = PageThumbnailFactory.Create(source.PageContent);
                    return await pageThumbnail.LoadThumbnailAsync(token);
                }
                else
                {
                    return new ThumbnailSource(ThumbnailType.Empty);
                }
            }
            else
            {
                return new ThumbnailSource(source.ThumbnailType);
            }
        }

        private async Task<ArchiveThumbnailSource> GetArchiveThumbnailSourceAsync(CancellationToken token)
        {
            if (_selectedContent is null)
            {
                var entry = await CreateRegularEntryAsync(Entry, token);
                var selected = await SelectAlternativeEntry(entry, token);
                _thumbnailType = GetThumbnailType(selected);
                var factory = new PageContentFactory(BookMemoryService);
                _selectedContent = factory.CreatePageContent(selected, token);
            }

            return new ArchiveThumbnailSource(_thumbnailType, _selectedContent);
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

}
