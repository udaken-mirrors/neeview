using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{

    public class BitmapPageContent : PageContent<byte[]>
    {
        //private PictureInfo? _pictureInfo;
        //private BitmapInfo _bitmapInfo = BitmapInfo.Default;


        public BitmapPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, new MemoryPageSource(archiveEntry), bookMemoryService)
        {
        }


        protected override void OnPageSourceChanged()
        {
            if (Data is null) return;

            if (PictureInfo is null)
            {
                try
                {
                    NVDebug.AssertMTA();
                    SetPictureInfo(CreatePictureInfo(Data));
                }
                catch (Exception ex)
                {
                    SetErrorMessage(ex.Message);
                    SetPictureInfo(new PictureInfo());
                }
            }

            // TODO: これはベースクラスに吸収できそう
            if (IsFailed)
            {
                Size = DefaultSize;
            }
            else
            {
                Size = PictureInfo.Size;
            }
        }

        // TODO: PictureInfoを拡張子てBitmapInfoやMediaInfoを作成する
        public PictureInfo CreatePictureInfo(byte[] data)
        {
            //if (this.PictureInfo != null) return this.PictureInfo;
            //token.ThrowIfCancellationRequested();

            var pictureInfo = new PictureInfo();

            using (var stream = new MemoryStream(data)) // _streamSource.CreateStream(token))
            {
                stream.Seek(0, SeekOrigin.Begin);
                var bitmapInfo = BitmapInfo.Create(stream, true);
                pictureInfo.BitmapInfo = bitmapInfo;
                var originalSize = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
                pictureInfo.OriginalSize = originalSize;

                var maxSize = bitmapInfo.IsTranspose ? Config.Current.Performance.MaximumSize.Transpose() : Config.Current.Performance.MaximumSize;
                var size = (Config.Current.Performance.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                pictureInfo.Size = size.IsEmpty ? originalSize : size;
                pictureInfo.AspectSize = bitmapInfo.IsTranspose ? bitmapInfo.GetAspectSize().Transpose() : bitmapInfo.GetAspectSize();

                //pictureInfo.Decoder = _streamSource.Decoder ?? ".NET BitmapImage";
                pictureInfo.Decoder = ".NET BitmapImage";
                pictureInfo.BitsPerPixel = bitmapInfo.BitsPerPixel;
                pictureInfo.Metadata = bitmapInfo.Metadata;

                //this.PictureInfo = pictureInfo;
            }

            return pictureInfo;
        }



#if false
        private BitmapInfo CreateBitmapInfo(byte[] data)
        {
            return BitmapInfo.Create(new MemoryStream(data));
#if false
            // TODO: AutoRotateとかどうなん？
            var bitmap = BitmapFrame.Create(new MemoryStream(data), BitmapCreateOptions.DelayCreation | BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
            return new BitmapInfo(bitmap);
#endif
        }
#endif
    }



}
