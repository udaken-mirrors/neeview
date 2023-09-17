using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ImagePageThumbnail : PageThumbnail
    {
        private readonly PageContent _content;
        private readonly IPictureSource _source;

        public ImagePageThumbnail(PageContent content, IPictureSource source) : base(content)
        {
            _content = content;
            _source = source;
        }

        public override async Task<ThumbnailSource> LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            NVDebug.AssertMTA();

            byte[]? thumbnailRaw = null;

            if (_content.IsFailed)
            {
                thumbnailRaw = null;
            }
            else
            {
                try
                {
                    await _content.LoadAsync(token);
                    var data = (_content.Data as IHasRawData)?.RawData;
                    if (data != null)
                    {
                        thumbnailRaw = MemoryControl.Current.RetryFuncWithMemoryCleanup(() => _source.CreateThumbnail(data, ThumbnailProfile.Current, token));
                    }
                }
                catch
                {
                    // NOTE: サムネイル画像取得失敗時はEmptyなサムネイル画像を適用する
                }
            }

            token.ThrowIfCancellationRequested();
            return new ThumbnailSource(thumbnailRaw);
        }
    }
}
