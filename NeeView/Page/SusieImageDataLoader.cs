using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NeeView.Susie;

namespace NeeView
{
    public class SusieImageDataLoader : IImageDataLoader
    {
        public async Task<ImageData> LoadAsync(ArchiveEntry entry, bool createPictureInfo, CancellationToken token)
        {
            if (!entry.IsIgnoreFileExtension && !PictureProfile.Current.IsSusieSupported(entry.Link ?? entry.EntryName))
            {
                return ImageData.CreateError("not support format");
            }

            try
            {
                if (entry.IsFileSystem)
                {
                    var path = entry.Link ?? entry.GetFileSystemPath();
                    if (path is null) throw new InvalidOperationException();
                    return CreateImageDataSource(await LoadFromFileAsync(path, entry, token), createPictureInfo);
                }
                else
                {
                    using (var stream = entry.OpenEntry())
                    {
                        return CreateImageDataSource( await LoadFromStreamAsync(stream, entry,  token), createPictureInfo);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ImageData.CreateError(ex.Message);
            }
        }

        // Bitmap読み込み(stream)
        private async Task<SusieImage?> LoadFromStreamAsync(Stream stream, ArchiveEntry entry, CancellationToken token)
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
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buff = ms.ToArray();
                }
            }

            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var result = accessor.GetPicture(entry.RawEntryName, buff, !entry.IsIgnoreFileExtension); // TODO: await
            await Task.CompletedTask;

            return result;
        }

        // Bitmap読み込み(ファイル版)
        private async Task<SusieImage?> LoadFromFileAsync(string fileName, ArchiveEntry entry, CancellationToken token)
        {
            var accessor = SusiePluginManager.Current.GetImagePluginAccessor();
            var result = accessor.GetPicture(fileName, null, !entry.IsIgnoreFileExtension); // TODO: await
            await Task.CompletedTask;

            return result;
        }

        private ImageData CreateImageDataSource(SusieImage? susieImage, bool createPictureInfo)
        {
            if (susieImage == null || susieImage.Plugin == null || susieImage.BitmapData == null)
            {
                return ImageData.CreateError("SusieIOException");
            }
            else
            {
                var pictureInfo = createPictureInfo ? PictureInfo.Create(susieImage.BitmapData, susieImage.Plugin.Name) : null;
                return ImageData.Create(susieImage.BitmapData, pictureInfo, this);
            }
        }
    }
}
