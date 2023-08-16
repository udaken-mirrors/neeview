using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NeeView.ComponentModel;
using NeeView.Media.Imaging;
using PhotoSauce.MagicScaler;

namespace NeeView
{

    public class BitmapViewSource : ViewSource
    {
        private BitmapPageContent _pageContent;
        private Picture _picture;

        public BitmapViewSource(IPageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent, bookMemoryService)
        {
            _pageContent = pageContent as BitmapPageContent ?? throw new ArgumentException("need BitmapPageContent", nameof(pageContent));
            _picture = new Picture(new BitmapPictureSource(_pageContent));
        }

        public override bool IsMemoryLocked => _pageContent.IsMemoryLocked;

        public Picture Picture => _picture;

        public override async Task LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            UpdatePicture(data, size, token);
            SetData(_picture.ImageSource, _picture.GetMemorySize(), null);
            await Task.CompletedTask;
        }

        public override void Unload()
        {
            if (Data is not null)
            {
                _picture.ClearImageSource();
                SetData(null, 0, null);
            }
        }

        private void UpdatePicture(DataSource data, Size size, CancellationToken token)
        {
            var rawData = data.Data as byte[];
            var pictureInfo = _pageContent.PictureInfo;
            if (rawData is null || pictureInfo is null) return;

            // NOTE: 非同期で処理されることを期待している
            NVDebug.AssertMTA();

            var result = _picture.CreateImageSource(rawData, size, token);
            Debug.Assert(_picture.ImageSource is not null);

            // [DEV]
            if (pictureInfo is not null && _picture.ImageSource is not null)
            {
                var requestSize = size;
                var sourceSize = pictureInfo.Size;
                var pictureSize = new Size(_picture.ImageSource.GetPixelWidth(), _picture.ImageSource.GetPixelHeight());
                Debug.WriteLine($"CreateBitmapImage: {_pageContent.ArchiveEntry}: {sourceSize:f0}: {requestSize:f0} -> {pictureSize:f0}");
            }
        }
    }

}