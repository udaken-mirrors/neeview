using NeeView.Collections.Generic;
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
    public class MediaPlayerCanvas : Grid,  IDisposable
    {
        private SimpleMediaPlayer _player;
        private DrawingBrush _videoBrush;
        private Rectangle _videoLayer;
        private ImageBrush _imageBlush;
        private Rectangle _imageLayer;
        private TextBlock _errorMessageTextBlock;
        private bool _disposedValue;


        public MediaPlayerCanvas(MediaViewData source, Rect viewbox, SimpleMediaPlayer player)
        {
            Debug.WriteLine($"Create.MediaPlayer: {source.Path}");

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



        protected virtual void Dispose(bool disposing)
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
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

        public void SetViewbox(Rect viewbox)
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
