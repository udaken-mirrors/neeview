using NeeView.ComponentModel;
using NeeView.Media.Imaging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ImageViewSourceStrategy : IViewSourceStrategy
    {
        private readonly PageContent _pageContent;
        private readonly Picture _picture;


        public ImageViewSourceStrategy(PageContent pageContent, IPictureSource pictureSource)
        {
            _pageContent = pageContent;
            _picture = new Picture(pictureSource);
        }


        /// <summary>
        /// 要求サイズで生成されているかチェック
        /// </summary>
        public bool CheckLoaded(Size size)
        {
            return _picture.IsCreated(size);
        }


        public async Task<DataSource> LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            // NOTE: 非同期で処理されることを期待している
            NVDebug.AssertMTA();

            var pictureInfo = _pageContent.PictureInfo;

            if (data is not null && pictureInfo is not null)
            {
                var rawData = (data.Data as IHasRawData)?.RawData;
                if (rawData is null) throw new InvalidOperationException($"No elements required for image generation");
                _picture.CreateImageSource(rawData, size, token);
                await Task.CompletedTask;

                // [DEV]
                if (pictureInfo is not null && _picture.ImageSource is not null)
                {
                    var requestSize = size;
                    var sourceSize = pictureInfo.Size;
                    var pictureSize = new Size(_picture.ImageSource.GetPixelWidth(), _picture.ImageSource.GetPixelHeight());
                    Debug.WriteLine($"CreateBitmapImage: {_pageContent.Entry}: {sourceSize:f0}: {requestSize:f0} -> {pictureSize:f0}");
                }

                Debug.Assert(_picture.ImageSource is not null);
                return new DataSource(new ImageViewData(_picture.ImageSource), _picture.GetMemorySize(), null);
            }
            else
            {
                return new DataSource(null, 0, null);
            }
        }

        public void Unload()
        {
            _picture.ClearImageSource();
        }
    }
}
