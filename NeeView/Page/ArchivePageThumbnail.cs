using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public class ArchivePageThumbnail : PageThumbnail
    {
        private ArchivePageContent _content;

        public ArchivePageThumbnail(ArchivePageContent content) : base(content)
        {
            _content = content;
        }

        public override async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NVDebug.AssertMTA();

            var source = await _content.GetArchiveThumbnailSourceAsync(token);
            if (source.ThumbnailType == ThumbnailType.Unique)
            {
                var pageThumbnail = PageThumbnailFactory.Create(source.PageContent);
                return await pageThumbnail.LoadThumbnailAsync(token);
            }
            else
            {
                return new ThumbnailSource(source.ThumbnailType);
            }
        }
    }

}
