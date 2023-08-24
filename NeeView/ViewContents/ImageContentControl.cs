using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NeeView.PageFrames;

namespace NeeView
{
    public class ImageContentControl : ContentControl, IDisposable, IHasImageSource
    {
        private PageFrameElement _element;
        private ImageSource _image;
        private ViewContentSize _contentSize;
        private Rectangle _rectangle;
        private bool _disposedValue;


        public ImageContentControl(PageFrameElement source, ImageSource image, ViewContentSize contentSize)
        {
            _element = source;
            _image = image;
            _contentSize = contentSize;

            _rectangle = new Rectangle();
            _rectangle.Fill = CreatePageImageBrush(true);
            UpdateBitmapScalingMode();

            this.Content = _rectangle;
            this.Width = _contentSize.LayoutSize.Width;
            this.Height = _contentSize.LayoutSize.Height;

            _contentSize.SizeChanged += ContentSize_SizeChanged;
        }


        public ImageSource ImageSource => _image;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _contentSize.SizeChanged -= ContentSize_SizeChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void ContentSize_SizeChanged(object? sender, EventArgs e)
        {
            UpdateBitmapScalingMode();
        }


        private ImageBrush CreatePageImageBrush(bool isStretch)
        {
            var brush = new ImageBrush();
            brush.ImageSource = _image;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = isStretch ? Stretch.Fill : Stretch.None;
            brush.TileMode = TileMode.None;
            brush.Viewbox = _element.ViewSizeCalculator.GetViewBox();
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }


        private void UpdateBitmapScalingMode()
        {
            var imageSize = _image is BitmapSource bitmapSource ? new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight) : new Size(_image.Width, _image.Height);

            // 画像サイズがビッタリの場合はドットバイドットになるような設定
            if (_contentSize.IsRightAngle && Math.Abs(_contentSize.PixelSize.Width - imageSize.Width) < 1.1 && Math.Abs(_contentSize.PixelSize.Height - imageSize.Height) < 1.1)
            {
                Debug.WriteLine($"OO: NearestNeighbor: {_element.Page}: {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.NearestNeighbor);
                _rectangle.SnapsToDevicePixels = true;
            }
            // DotKeep mode
            // TODO: Config.Current参照はよろしくない
            else if (Config.Current.ImageDotKeep.IsImgeDotKeep(_contentSize.PixelSize, imageSize))
            {
                Debug.WriteLine($"XX: NearestNeighbor: {_element.Page}: {imageSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.NearestNeighbor);
                _rectangle.SnapsToDevicePixels = true;
            }
            else
            {
                Debug.WriteLine($"XX: Fant: {_element.Page}: {imageSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.Fant);
                _rectangle.SnapsToDevicePixels = false;
            }
        }


    }
}
