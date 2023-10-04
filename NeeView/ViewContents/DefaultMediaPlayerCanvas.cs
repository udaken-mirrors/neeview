using NeeView.Collections.Generic;
using NeeView.PageFrames;
using NeeView.Susie;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    public class DefaultMediaPlayerCanvas : MediaPlayerCanvas, IDisposable
    {
        private readonly DefaultMediaPlayer _player;
        private readonly DrawingBrush _videoBrush;
        private readonly Rectangle _videoLayer;
        private readonly ImageBrush _imageBlush;
        private readonly Rectangle _imageLayer;
        private readonly TextBlock _errorMessageTextBlock;
        private bool _disposedValue;


        public DefaultMediaPlayerCanvas(PageFrameElement element, MediaViewData source, ViewContentSize contentSize, Rect viewbox, DefaultMediaPlayer player)
        {
            Debug.WriteLine($"Create.MediaPlayer: {source.MediaSource}");

            _player = player;
            _player.MediaPlayed += Player_MediaPlayed;
            _player.MediaFailed += Player_MediaFailed;

            var videoDrawing = new VideoDrawing()
            {
                //Rect = new Rect(size),
                Player = _player.Player,
                Rect = new Rect(0, 0, 256, 256), // Stretch.Fill で引き伸ばすので値は適応でも良い
            };

            _videoBrush = new DrawingBrush()
            {
                Drawing = videoDrawing,
                Stretch = Stretch.Fill,
                Viewbox = viewbox,
            };

            _videoLayer = new Rectangle()
            {
                Fill = _videoBrush,
                Visibility = Visibility.Hidden,
            };

            _imageBlush = new ImageBrush()
            {
                ImageSource = source.ImageSource,
                Stretch = Stretch.Fill,
                Viewbox = viewbox,
            };

            _imageLayer = new Rectangle()
            {
                Fill = _imageBlush,
            };

            _errorMessageTextBlock = new TextBlock()
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Padding = new Thickness(40, 20, 40, 20),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed,
            };

            // root grid
            this.Background = Brushes.Black;
            this.Children.Add(_videoLayer);
            if (source.ImageSource is not null)
            {
                this.Children.Add(_imageLayer);
            }
            this.Children.Add(_errorMessageTextBlock);
        }



        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _player.MediaPlayed -= Player_MediaPlayed;
                    _player.MediaFailed -= Player_MediaFailed;
                }
                _disposedValue = true;
            }
            base.Dispose(disposing);
        }

        private void Player_MediaPlayed(object? sender, EventArgs e)
        {
            ShowVideo();
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            if (_errorMessageTextBlock is null) return;

            _videoLayer.Visibility = Visibility.Collapsed;
            _imageLayer.Visibility = Visibility.Collapsed;

            _errorMessageTextBlock.Text = e.ErrorException.Message;
            _errorMessageTextBlock.Visibility = Visibility.Visible;
        }

        public override void SetViewbox(Rect viewbox)
        {
            _videoBrush.Viewbox = viewbox;
            _imageBlush.Viewbox = viewbox;
        }

        private void ShowVideo()
        {
            _videoLayer.Visibility = Visibility.Visible;
            _imageLayer.Visibility = Visibility.Collapsed;
        }
    }
}
