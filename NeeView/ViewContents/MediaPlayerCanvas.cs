using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace NeeView
{
    public class MediaPlayerCanvas : Grid, IDisposable
    {
        private ObjectPool<MediaPlayer> _mediaPlayerPool;
        private MediaPlayer _player;
        private DrawingBrush _videoBrush;
        private Rectangle _videoLayer;
        private ImageBrush _imageBlush;
        private Rectangle _imageLayer;
        private TextBlock _errorMessageTextBlock;
        private bool _disposedValue;


        public MediaPlayerCanvas(MediaSource source, Rect viewbox, ObjectPool<MediaPlayer> mediaPlayerPool)
        {
            Debug.WriteLine($"Create.MediaPlayer: {source.Path}");

            _mediaPlayerPool = mediaPlayerPool;

            var videoDrawing = new VideoDrawing()
            {
                //Rect = new Rect(size),
                Rect = new Rect(0, 0, 256, 256), // Stretch.Fill で引き伸ばすので値は適応でも良い
            };

            _videoBrush = new DrawingBrush()
            {
                Drawing = videoDrawing,
                Stretch = Stretch.Fill,
                Viewbox = viewbox, //Element.ViewSizeCalculator.GetViewBox(),
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

            // NOTE: コントロールの存在を保証するためにプレイヤー生成は最後に行う
            _player = OpenMediaPlayer(new Uri(source.Path));
            videoDrawing.Player = _player;
        }


        public event EventHandler? MediaPlayed;


        public bool IsMuted
        {
            get => _player.IsMuted;
            set => _player.IsMuted = value;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_player is not null)
                    {
                        CloseMediaPlayer(_player);
                    }
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void Play()
        {
            _player.Play();
        }

        public void Pause()
        {
            _player.Pause();
        }

        public void SetViewbox(Rect viewbox)
        {
            _videoBrush.Viewbox = viewbox;
            _imageBlush.Viewbox = viewbox;
        }

        private MediaPlayer OpenMediaPlayer(Uri uri)
        {
            var player = _mediaPlayerPool.Allocate();
            player.MediaOpened += Player_MediaOpened;
            player.MediaEnded += Player_MediaEnded;
            player.MediaFailed += Player_MediaFailed;
            player.Open(uri);
            player.Play();
            player.IsMuted = true;
            return player;
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            // NOTE: 動画再生開始時のちらつき軽減
            DelayAction(ShowVideo, 16);
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            var player = sender as MediaPlayer ?? throw new InvalidOperationException();

            // ループ再生
            player.Position = TimeSpan.FromMilliseconds(1);
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            if (_errorMessageTextBlock is null) return;

            _errorMessageTextBlock.Text = e.ErrorException.Message;
            _errorMessageTextBlock.Visibility = Visibility.Visible;
        }

        private void CloseMediaPlayer(MediaPlayer player)
        {
            player.MediaOpened -= Player_MediaOpened;
            player.MediaEnded -= Player_MediaEnded;
            player.MediaFailed -= Player_MediaFailed;
            player.Stop();

            // NOTE: 一瞬黒い画像が表示されるのを防ぐために開放タイミングをずらす。今作では不要か？
            DelayAction(() =>
            {
                player.Close();
                _mediaPlayerPool.Release(player);
            }, 16);
        }

        private void ShowVideo()
        {
            _videoLayer.Visibility = Visibility.Visible;
            _imageLayer.Visibility = Visibility.Collapsed;
            MediaPlayed?.Invoke(this, EventArgs.Empty);
        }


        // TODO: 汎用化？
        private static void DelayAction(Action action, int delayMicroseconds)
        {
            AppDispatcher.BeginInvoke(async () =>
            {
                await Task.Delay(delayMicroseconds);
                action();
            });
        }

        // TODO: 汎用化？
        private static void DelayCycleAction(Action action, int delayCycle)
        {
            int count = 0;
            CompositionTarget.Rendering += OnRendering;
            void OnRendering(object? sender, EventArgs e)
            {
                if (++count >= delayCycle)
                {
                    CompositionTarget.Rendering -= OnRendering;
                    action();
                }
            }
        }


    }

}
