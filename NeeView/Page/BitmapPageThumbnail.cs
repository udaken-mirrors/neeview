using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BitmapPageThumbnail : PageThumbnail
    {
        private BitmapPageContent _content;

        public BitmapPageThumbnail(BitmapPageContent content) : base(content)
        {
            _content = content;
        }

        /// <summary>
        /// サムネイルロード
        /// </summary>
        public override async Task LoadThumbnailAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            //if (_disposedValue) return;

            await Thumbnail.InitializeAsync(_content.Entry, null, token);
            if (Thumbnail.IsValid) return;


            //var source = LoadPictureSource(token);
            var source = new BitmapPictureSource(_content);

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
                    var data = _content.Data as byte[];
                    if (data != null)
                    {
                        thumbnailRaw = MemoryControl.Current.RetryFuncWithMemoryCleanup(() => source.CreateThumbnail(data, ThumbnailProfile.Current, token));
                    }
                }
                catch
                {
                    // NOTE: サムネイル画像取得失敗時はEnptyなサムネイル画像を適用する
                }
            }

            token.ThrowIfCancellationRequested();
            Thumbnail.Initialize(thumbnailRaw);
        }
    }



}
