using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace NeeView
{
    public class ArchivePageThumbnail : PageThumbnail
    {
        private readonly ArchivePageContent _content;

        public ArchivePageThumbnail(ArchivePageContent content) : base(content)
        {
            _content = content;
            this.Thumbnail.IsCacheEnabled = true;
        }

        public override async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NVDebug.AssertMTA();

            var pageContent = await ArchivePageUtility.GetSelectedPageContentAsync(_content.ArchiveEntry, token);
            if (_content.ArchiveEntry.IsMedia())
            {
                return new ThumbnailSource(ThumbnailType.Media);
            }
            else if (pageContent is null)
            {
                return new ThumbnailSource(ThumbnailType.Empty);
            }
            else
            {
                Debug.Assert(pageContent is not ArchivePageContent);
                var pageThumbnail = PageThumbnailFactory.Create(pageContent);
                return await pageThumbnail.LoadThumbnailAsync(token);
            }
        }
    }

}
