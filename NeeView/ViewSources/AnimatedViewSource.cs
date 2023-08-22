using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class AnimatedViewSource : ViewSource
    {
        private AnimatedPageContent _pageContent;

        public AnimatedViewSource(PageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, bookMemoryService)
        {
            _pageContent = pageContent as AnimatedPageContent ?? throw new ArgumentException("need AnimatedPageContent", nameof(pageContent));
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
                var path = data.Data as string ?? throw new InvalidOperationException(nameof(data));
                var image = LoadImage(path);
                var source = new MediaSource(path, image);
                SetData(source, 0, null);
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
                Debug.WriteLine($"ImageLoadFailled: {ex.Message}");
                return null;
            }
        }

        public override void Unload()
        {
            if (Data is not null)
            {
                SetData(null, 0, null);
            }
        }
    }
}
