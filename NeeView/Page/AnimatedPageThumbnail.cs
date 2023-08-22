using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace NeeView
{
    public class AnimatedPageThumbnail : PageThumbnail
    {
        private AnimatedPageContent _content;


        public AnimatedPageThumbnail(AnimatedPageContent content) : base(content)
        {
            _content = content;
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
                    if (_content.Data is not null && _content.PictureInfo is not null)
                    {
                        var path = (string)_content.Data;
                        using (var stream = File.OpenRead(path))
                        {
                            thumbnailRaw = ThumbnailTools.CreateThumbnailImage(stream, _content.PictureInfo, ThumbnailProfile.Current, token);
                        }
                    }
                }
                catch
                {
                    // NOTE: サムネイル画像取得失敗時はEnptyなサムネイル画像を適用する
                }
            }

            token.ThrowIfCancellationRequested();
            return new ThumbnailSource(thumbnailRaw);
        }
    }
}