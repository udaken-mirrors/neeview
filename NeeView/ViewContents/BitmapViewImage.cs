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
    public class BitmapViewImage : ContentControl, IDisposable
    {
        private PageFrameElement _element;
        private BitmapSource _image;
        private ViewContentSize _contentSize;
        private Rectangle _rectangle;
        private bool _disposedValue;


        public BitmapViewImage(PageFrameElement source, BitmapSource bitmap, ViewContentSize contentSize)
        {
            _element = source;
            _image = bitmap;
            _contentSize = contentSize;

            _rectangle = new Rectangle();
            _rectangle.Fill = CreatePageImageBrush(true);
            UpdateBitmapScalingMode();

            this.Content = _rectangle;
            this.Width = _contentSize.LayoutSize.Width;
            this.Height = _contentSize.LayoutSize.Height;

            _contentSize.SizeChanged += ContentSize_SizeChanged;
        }


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
            var pictureSize = new Size(_image.Width, _image.Height);

            // 画像サイズがビッタリの場合はドットバイドットになるような設定
            if (_contentSize.IsRightAngle && Math.Abs(_contentSize.PixelSize.Width - _image.PixelWidth) < 1.1 && Math.Abs(_contentSize.PixelSize.Height - _image.PixelHeight) < 1.1)
            {
                Debug.WriteLine($"OO: NearestNeighbor: {_element.Page}: {pictureSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.NearestNeighbor);
                _rectangle.SnapsToDevicePixels = true;
            }
            // DotKeep mode
            // TODO: Config.Current参照はよろしくない
            else if (Config.Current.ImageDotKeep.IsImgeDotKeep(_contentSize.PixelSize, pictureSize))
            {
                Debug.WriteLine($"XX: NearestNeighbor: {_element.Page}: {pictureSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.NearestNeighbor);
                _rectangle.SnapsToDevicePixels = true;
            }
            else
            {
                Debug.WriteLine($"XX: Fant: {_element.Page}: {pictureSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.Fant);
                _rectangle.SnapsToDevicePixels = false;
            }
        }


    }
}
