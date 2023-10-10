using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows;
using NeeView.ComponentModel;

namespace NeeView
{
    public class ArchivePageContent : PageContent
    {
        public ArchivePageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
            Thumbnail.IsCacheEnabled = true;
        }

        public override bool IsFileContent => true;

        public Thumbnail Thumbnail { get; } = new Thumbnail();


        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                var pictureInfo = new PictureInfo(DefaultSize);

                var data = await LoadArchivePageData(token);
                if (data is null)
                {
                    return PageSource.CreateEmpty();
                }
                else if (data.PageContent?.IsFailed == true)
                {
                    return new ArchivePageSource(null, data.PageContent.ErrorMessage, pictureInfo);
                }
                else
                {
                    return new ArchivePageSource(data, null, pictureInfo);
                }
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

        private async Task<ArchivePageData?> LoadArchivePageData(CancellationToken token)
        {
            var pageContent = await ArchivePageUtility.GetSelectedPageContentAsync(ArchiveEntry, token);
            if (pageContent is null)
            {
                if (ArchiveEntry.IsMedia())
                {
                    return new ArchivePageData(ArchiveEntry, ThumbnailType.Media, null, null);
                }
                else
                {
                    return new ArchivePageData(ArchiveEntry, ThumbnailType.Empty, null, null);
                }
            }
            else
            {
                Debug.Assert(pageContent is not ArchivePageContent);
                await pageContent.LoadAsync(token);
                token.ThrowIfCancellationRequested();
                var dataSource = pageContent.CreateDataSource();
                if (dataSource.DataState == DataState.None) return null;
                return new ArchivePageData(ArchiveEntry, ThumbnailType.Unique, pageContent, dataSource);
            }
        }
    }

    public class ArchivePageSource : PageSource
    {
        public ArchivePageSource(ArchivePageData? data, string? errorMessage, PictureInfo? pictureInfo) : base(data, errorMessage, pictureInfo)
        {
        }

        public override long DataSize => (Data as ArchivePageData)?.PageContent?.DataSize ?? 0;
    }
}
