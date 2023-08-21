using MediaInfoLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace NeeView
{
    public class MediaPageContent : PageContent
    {
        public MediaPageContent(ArchiveEntry archiveEntry, BookMemoryService? bookMemoryService)
            : base(archiveEntry, bookMemoryService)
        {
        }


        public override async Task<PageSource> LoadSourceAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();

            try
            {
                // ArchvieFileの場合はTempFile化
                var fileProxy = Entry.GetFileProxy(); // TODO: async化
                var pictureInfo = CreatePictureInfo(fileProxy.Path); // TODO: Async
                await Task.CompletedTask;
                return new PageSource(fileProxy.Path, null, pictureInfo);
            }
            catch (OperationCanceledException)
            {
                return PageSource.CreateEmpty();
            }
            catch (Exception ex)
            {
                return new PageSource(null, ex.Message, new PictureInfo(new Size(1920, 1080)));
            }
        }

        private PictureInfo CreatePictureInfo(string path)
        {
            int width = 1920;
            int height = 1080;

            // 幅と高さを非同期で得るために MediaInfoLib を使用している
            var mediaInfo = new MediaInfoLib.MediaInfo();
            if (!mediaInfo.IsEnabled) throw new ApplicationException("Cannnot load MediaInfo.dll");

            int result = mediaInfo.Open(path);
            if (result == 0) throw new IOException($"Cannot open MediaInfo: {path}");

            int videoCount = mediaInfo.Count_Get(StreamKind.Video);
            if (videoCount > 0)
            {
                width = int.Parse(mediaInfo.Get(StreamKind.Video, 0, "Width"));
                height = int.Parse(mediaInfo.Get(StreamKind.Video, 0, "Height"));
            }
            else
            {
                int imageCount = mediaInfo.Count_Get(StreamKind.Image);
                if (imageCount > 0)
                {
                    width = int.Parse(mediaInfo.Get(StreamKind.Image, 0, "Width"));
                    height = int.Parse(mediaInfo.Get(StreamKind.Image, 0, "Height"));
                }
            }

            var pictureInfo = new PictureInfo();
            pictureInfo.OriginalSize = new Size(width, height);
            pictureInfo.Size = pictureInfo.OriginalSize;

            return pictureInfo;
        }

    }

}
