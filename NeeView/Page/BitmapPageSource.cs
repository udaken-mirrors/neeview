using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class BitmapPageSource : PageSource
    {
        public BitmapPageSource(ArchiveEntry entry) : base(entry)
        {
        }


        public IImageDataLoader? ImageDataLoader { get; private set; }

        public PictureInfo? PictureInfo { get; private set; }

        public byte[]? DataBytes => (byte[]?)Data;

        public override long DataSize => DataBytes?.LongLength ?? 0;


        /// <summary>
        /// 読み込み。キャンセル等された場合でも正常終了する。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override async Task LoadAsyncCore(CancellationToken token)
        {
            try
            {
                //Debug.WriteLine($"Loading...: {ArchiveEntry}");
#if DEBUG
                if (Debugger.IsAttached)
                {
                    NVDebug.AssertMTA();
                    await Task.Delay(200, token);
                }
#endif
                NVDebug.AssertMTA();

                var loader = ImageDataLoader ?? NeeView.ImageDataLoader.Default;
                var createPictureInfo = PictureInfo is null;

                var imageData = await loader.LoadAsync(ArchiveEntry, createPictureInfo, token);

                ImageDataLoader = imageData.ImageDataLoader;
                PictureInfo = PictureInfo ?? imageData.PictureInfo;
                SetData(imageData.Data, imageData.ErrorMessage);
            }
            catch (OperationCanceledException)
            {
                //Debug.WriteLine($"Load Canceled: {ArchiveEntry}");
                SetData(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                SetData(null, ex.Message);
                throw;
            }
        }

        protected override void UnloadCore()
        {
            if (!IsFailed)
            {
                SetData(null, null);
            }
        }
    }
}
