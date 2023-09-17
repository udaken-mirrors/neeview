using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageThumbnail
    {
        private readonly PageContent _content;

        public PageThumbnail(PageContent content)
        {
            _content = content;
        }

        public Thumbnail Thumbnail { get; } = new Thumbnail();


        public async Task LoadAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            if (Thumbnail.IsValid) return;

            await Thumbnail.InitializeFromCacheAsync(_content.Entry, null, token);
            if (Thumbnail.IsValid) return;

            var source = await LoadThumbnailAsync(token);
            Thumbnail.Initialize(source);
        }

        public virtual async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            // ダミーサムネイル
            return await Task.FromResult(new ThumbnailSource(null));
        }
    }



}
