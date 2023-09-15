using System.Diagnostics;
using System.Threading;
using System.Windows;
using NeeView.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Pictureの生成を管理
    /// </summary>
    public class PictureContent
    {
        private readonly PageContent _pageContent;
        private readonly Picture _picture;


        public PictureContent(PageContent pageContent, IPictureSource pictureSource)
        {
            _pageContent = pageContent;
            _picture = new Picture(pictureSource);
        }


        public Picture Picture => _picture;


        public bool IsCreated(Size size)
        {
            return _picture.IsCreated(size);
        }

        /// <summary>
        /// 指定したサイズで Picture を生成する
        /// </summary>
        /// <param name="data"></param>
        /// <param name="size">指定サイズ</param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Picture Load(object? data, Size size, CancellationToken token)
        {
            // NOTE: 非同期で処理されることを期待している
            NVDebug.AssertMTA();

            var pictureInfo = _pageContent.PictureInfo;

            if (data is not null && pictureInfo is not null)
            {
                _picture.CreateImageSource(data, size, token);
                Debug.Assert(_picture.ImageSource is not null);

                // [DEV]
                if (pictureInfo is not null && _picture.ImageSource is not null)
                {
                    var requestSize = size;
                    var sourceSize = pictureInfo.Size;
                    var pictureSize = new Size(_picture.ImageSource.GetPixelWidth(), _picture.ImageSource.GetPixelHeight());
                    Debug.WriteLine($"CreateBitmapImage: {_pageContent.Entry}: {sourceSize:f0}: {requestSize:f0} -> {pictureSize:f0}");
                }
            }

            return _picture;
        }

        public void Unload()
        {
            _picture.ClearImageSource();
        }
    }
}