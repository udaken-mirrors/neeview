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
    public class MediaPageContent : PageContent<string>
    {
        //private PictureInfo? _pictureInfo;


        public MediaPageContent(ArchiveEntry archiveEntry, BookMemoryService bookMemoryService)
            : base(archiveEntry, new FilePageSource(archiveEntry), bookMemoryService)
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
                    Debug.WriteLine(ex);
                    SetErrorMessage(ex.Message);
                    SetPictureInfo(new PictureInfo() { Size = new Size(1920, 1080), OriginalSize = new Size(1920, 1080) });
                }
            }

            Size = PictureInfo.Size;
        }


        public PictureInfo CreatePictureInfo(string path)
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
