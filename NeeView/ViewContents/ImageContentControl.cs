using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NeeView.PageFrames;

namespace NeeView
{
    public class ImageContentControl : ContentControl, IDisposable, IHasImageSource
    {
        private readonly PageFrameElement _element;
        private readonly ImageSource _image;
        private readonly ViewContentSize _contentSize;
        private readonly Rectangle _rectangle;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;


        public ImageContentControl(PageFrameElement source, ImageSource image, ViewContentSize contentSize, PageBackgroundSource backgroundSource)
        {
            _element = source;
            _image = image;
            _contentSize = contentSize;

            var grid = new Grid();

            // background
            // TODO: アルファチャンネルを含む画像であるならば表示するようにする
            var background = new Rectangle();
            background.SetBinding(Rectangle.FillProperty, new Binding(nameof(PageBackgroundSource.Brush)) { Source = backgroundSource });
            background.Margin = new Thickness(1);
            background.HorizontalAlignment = HorizontalAlignment.Stretch;
            background.VerticalAlignment = VerticalAlignment.Stretch;
            grid.Children.Add(background);

            // image
            _rectangle = new Rectangle();
            _rectangle.Fill = CreatePageImageBrush(true);
            _rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            _rectangle.VerticalAlignment = VerticalAlignment.Stretch;
            grid.Children.Add(_rectangle);

            // image scaling mode
            UpdateBitmapScalingMode();

            this.Content =  grid;

            _contentSize.SizeChanged += ContentSize_SizeChanged;
        }


        public ImageSource ImageSource => _image;

        /// <summary>
        /// BitmapScaleMode指定。Printerで使用される。
        /// </summary>
        public BitmapScalingMode? ScalingMode
        {
            get { return _scalingMode; }
            set
            {
                if (_scalingMode != value)
                {
                    _scalingMode = value;
                    UpdateBitmapScalingMode();
                }
            }
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
            var imageSize = _image is BitmapSource bitmapSource ? new Size(bitmapSource.PixelWidth, bitmapSource.PixelHeight) : new Size(_image.Width, _image.Height);

            // ScalingMode が指定されている
            if (_scalingMode is not null)
            {
                Debug.WriteLine($"XX: Force {_scalingMode.Value}: {_element.Page}: {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, _scalingMode.Value);
                _rectangle.SnapsToDevicePixels = _scalingMode.Value == BitmapScalingMode.NearestNeighbor;
            }
            // 画像サイズがビッタリの場合はドットバイドットになるような設定
            else if (_contentSize.IsRightAngle && Math.Abs(_contentSize.PixelSize.Width - imageSize.Width) < 1.1 && Math.Abs(_contentSize.PixelSize.Height - imageSize.Height) < 1.1)
            {
                Debug.WriteLine($"OO: NearestNeighbor: {_element.Page}: {imageSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.NearestNeighbor);
                _rectangle.SnapsToDevicePixels = true;
            }
            // DotKeep mode
            // TODO: Config.Current参照はよろしくない
            else if (Config.Current.ImageDotKeep.IsImageDotKeep(_contentSize.PixelSize, imageSize))
            {
                Debug.WriteLine($"XX: NearestNeighbor: {_element.Page}: {imageSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.NearestNeighbor);
                _rectangle.SnapsToDevicePixels = true;
            }
            else
            {
                Debug.WriteLine($"XX: Fantastic: {_element.Page}: {imageSize:f0} != request {_contentSize.PixelSize:f0}");
                RenderOptions.SetBitmapScalingMode(_rectangle, BitmapScalingMode.Fant);
                _rectangle.SnapsToDevicePixels = false;
            }
        }




    }
}
