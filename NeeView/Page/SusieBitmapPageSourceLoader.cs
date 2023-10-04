using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NeeView.Susie;

namespace NeeView
{
    public class SusieBitmapPageSourceLoader : IBitmapPageSourceLoader
    {
        public async Task<BitmapPageSource> LoadAsync(ArchiveEntry entry, bool createPictureInfo, CancellationToken token)
        {
            if (!entry.IsIgnoreFileExtension && !PictureProfile.Current.IsSusieSupported(entry.Link ?? entry.EntryName))
            {
                return BitmapPageSource.CreateError("not support format");
            }

            try
            {
                if (entry.IsFileSystem)
                {
                    return CreateImageDataSource(await LoadFromFileAsync(entry, token), createPictureInfo);
                }
                else
                {
                    return CreateImageDataSource(await LoadFromStreamAsync(entry, token), createPictureInfo);
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
        private static async Task<SusieImage?> LoadFromStreamAsync(ArchiveEntry entry, CancellationToken token)
        {
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

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var result = accessor.GetPicture(entry.RawEntryName, buff, !entry.IsIgnoreFileExtension); // TODO: await
            await Task.CompletedTask;

            return result;
        }

        // Bitmap読み込み(ファイル版)
        private static async Task<SusieImage?> LoadFromFileAsync(ArchiveEntry entry, CancellationToken token)
        {
            var path = entry.Link ?? entry.GetFileSystemPath();
            if (path is null) throw new InvalidOperationException();

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var result = accessor.GetPicture(path, null, !entry.IsIgnoreFileExtension); // TODO: await
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
