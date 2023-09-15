using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class AnimatedViewSource : ViewSource, IHasImageSource
    {
        private readonly AnimatedPageContent _pageContent;
        private readonly PictureContent _pictureLoader;

        public AnimatedViewSource(PageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, bookMemoryService)
        {
            _pageContent = pageContent as AnimatedPageContent ?? throw new ArgumentException("need AnimatedPageContent", nameof(pageContent));
            _pictureLoader = new PictureContent(_pageContent, new BitmapPictureSource(_pageContent));
        }

        public ImageSource? ImageSource => (Data as MediaSource)?.ImageSource ?? _pictureLoader.Picture.ImageSource;


        public override bool CheckLoaded(Size size)
        {
            return IsLoaded && (Data is MediaSource || _pictureLoader.IsCreated(size));
        }

        public override async Task LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            NVDebug.AssertMTA();

            if (data.IsFailed)
            {
                SetData(null, 0, data.ErrorMessage);
            }
            else
            {
                if (data.Data is string path)
                {
                    var image = LoadImage(path);

                    // 色情報とBPP設定。
                    if (image is not null)
                    {
                        _pageContent.PictureInfo?.SetPixelInfo(image);
                    }

                    var source = new MediaSource(path, image);
                    SetData(source, 0, null);
                }
                else if (data.Data is byte[])
                {
                    _pictureLoader.Load(data.Data, size, token);
                    var picture = _pictureLoader.Picture;
                    SetData(picture.ImageSource, picture.GetMemorySize(), null);
                }
                else
                {
                    throw new InvalidOperationException(nameof(data.Data));
                }
            }
            await Task.CompletedTask;
        }

        private BitmapImage? LoadImage(string path)
        {
            try
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ImageLoadFailed: {ex.Message}");
                return null;
            }
        }

        public override void Unload()
        {
            if (Data is not null)
            {
                _pictureLoader.Unload();
                SetData(null, 0, null);
            }
        }
    }
}
