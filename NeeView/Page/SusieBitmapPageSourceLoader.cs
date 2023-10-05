using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NeeView.Susie;

namespace NeeView
{
    public class SusieBitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        public async Task<BitmapPageSource> LoadAsync(ArchiveEntryStreamSource streamSource, bool createPictureInfo, CancellationToken token)
        {
            var entry = streamSource.ArchiveEntry;
            if (!Config.Current.Image.Standard.IsAllFileSupported && !PictureProfile.Current.IsSusieSupported(entry.Link ?? entry.EntryName))
            {
                return BitmapPageSource.CreateError("not support format");
            }

            try
            {
                if (entry.IsFileSystem)
                {
                    return CreateImageDataSource(await LoadFromFileAsync(streamSource, token), createPictureInfo);
                }
                else
                {
                    return CreateImageDataSource(await LoadFromStreamAsync(streamSource, token), createPictureInfo);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return BitmapPageSource.CreateError(ex.Message);
            }
        }

        // Bitmap読み込み(stream)
        private static async Task<SusieImage?> LoadFromStreamAsync(ArchiveEntryStreamSource streamSource, CancellationToken token)
        {
            var entry = streamSource.ArchiveEntry;

            byte[] buff;
            using (var stream = streamSource.OpenStream())
            {
                buff = stream.ToArray(0, (int)entry.Length);
            }

#if false
            byte[] buff;
            var rawData = entry.GetRawData();
            if (rawData != null)
            {
                ////Debug.WriteLine($"SusiePictureStream: {entry.EntryLastName} from RawData");
                buff = rawData;
            }
            else
            {
                ////Debug.WriteLine($"SusiePictureStream: {entry.EntryLastName} from Stream");
                using var stream = entry.OpenEntry();
                buff = stream.ToArray(0, (int)entry.Length);
            }
#endif

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var isCheckExtension = !Config.Current.Image.Standard.IsAllFileSupported;
            var result = accessor.GetPicture(entry.RawEntryName, buff, isCheckExtension); // TODO: await
            await Task.CompletedTask;

            return result;
        }

        // Bitmap読み込み(ファイル版)
        private static async Task<SusieImage?> LoadFromFileAsync(ArchiveEntryStreamSource streamSource, CancellationToken token)
        {
            var entry = streamSource.ArchiveEntry;

            var path = entry.Link ?? entry.GetFileSystemPath();
            if (path is null) throw new InvalidOperationException();

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var isCheckExtension = !Config.Current.Image.Standard.IsAllFileSupported;
            var result = accessor.GetPicture(path, null, isCheckExtension); // TODO: await
            await Task.CompletedTask;

            return result;
        }

        private BitmapPageSource CreateImageDataSource(SusieImage? susieImage, bool createPictureInfo)
        {
            if (susieImage == null || susieImage.Plugin == null || susieImage.BitmapData == null)
            {
                return BitmapPageSource.CreateError("SusieIOException");
            }
            else
            {
                var streamSource = new MemoryStreamSource(susieImage.BitmapData);
                var pictureInfo = createPictureInfo ? PictureInfo.Create(streamSource, susieImage.Plugin.Name) : null;
                return BitmapPageSource.Create(new BitmapPageData(streamSource), pictureInfo, this);
            }
        }
    }
}
