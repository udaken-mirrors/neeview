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

            await _content.LoadAsync(token);
            var thumbnail = (_content.Data as ArchivePageData)?.Thumbnail;

            return thumbnail?.CreateSource() ?? new ThumbnailSource(ThumbnailType.Unique);
        }
    }

}
