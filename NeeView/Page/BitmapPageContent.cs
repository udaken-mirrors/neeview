using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{

    public class BitmapPageContent : PageContent
    {

        public BitmapPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, new BitmapPageSource(archiveEntry), bookMemoryService)
        {
        }


        public new byte[]? Data => (byte[]?)base.Data;


        protected override void OnPageSourceChanged()
        {
            if (Data is null) return;

            if (PictureInfo is null)
            {
                NVDebug.AssertMTA();
                SetPictureInfo(((BitmapPageSource)PageSource).PictureInfo);
            }

            // TODO: これはベースクラスに吸収できそう
            if (IsFailed)
            {
                Size = DefaultSize;
            }
            else
            {
                Size = PictureInfo?.Size ?? DefaultSize;
            }
        }

    }



    public static class PageThumbnailFactory
    {
        public static PageThumbnail Create(PageContent content)
        {
            switch (content)
            {
                case BitmapPageContent bitmapPageContent:
                    return new BitmapPageThumbnail(bitmapPageContent);

                default:
                    // not support yet.
                    return new PageThumbnail(content);
            }
        }
    }


    public class PageThumbnail
    {
        private PageContent _content;

        public PageThumbnail(PageContent content)
        {
            _content = content;
        }

        public Thumbnail Thumbnail { get; } = new Thumbnail();

        public virtual async Task LoadThumbnailAsync(CancellationToken token)
        {
            await Task.CompletedTask;
        }
    }

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
                    var data = _content.Data;
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
