using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;


namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class MediaViewContent : ViewContent, INotifyPropertyChanged
    {
        // TODO: static ダメ！Book切り替えで開放されるように！
        private static ObjectPool<MediaPlayer> _mediaPlayerPool = new();

        private MediaPlayer? _player;
        private TextBlock? _errorMessageTextBlock;
        private Visibility _videoVisibility = Visibility.Hidden;

        private DisposableCollection _disposables = new();
        private bool _disposedValue;


        public MediaViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity)
            : base(element, scale, viewSource, activity)
        {
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public Visibility VideoVisibility
        {
            get { return _videoVisibility; }
            set { SetProperty(ref _videoVisibility, value); }
        }


        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_player is not null)
                    {
                        CloseMediaPlayer(_player);
                        _player = null;
                    }

                    _disposables.Dispose();
                    this.Content = null;
                }
                _disposedValue = true;
            }

            base.Dispose(disposing);
        }


        public override void Initialize()
        {
            base.Initialize();

            _disposables.Add(Activity.SubscribePropertyChanged((s, e) => UpdateVideoStatus()));
        }


        protected override FrameworkElement CreateLoadedContent(Size size, object data)
        {
            var path = data as string ?? throw new InvalidOperationException();
            return CreateMediaContent(path);
        }

        private MediaPlayer OpenMediaPlayer(Uri uri)
        {
            var player = _mediaPlayerPool.Allocate();
            player.MediaOpened += Player_MediaOpened;
            player.MediaEnded += Player_MediaEnded;
            player.MediaFailed += Player_MediaFailed;
            player.Open(uri);
            return player;
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            // NOTE: 動画再生開始時のちらつき軽減
            DelayCycleAction(ShowVideo, 3);
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
            //DelayCycleAction(() => player.Close(), 3);
            player.Close();
            _mediaPlayerPool.Release(player);
        }

        private void ShowVideo()
        {
            VideoVisibility = Visibility.Visible;
            UpdateVideoStatus();
        }

        private void UpdateVideoStatus()
        {
            if (_player is null) return;
            if (VideoVisibility != Visibility.Visible) return;

            _player.IsMuted = !Activity.IsSelected;

            if (Activity.IsVisible)
            {
                _player.Play();
            }
            else
            {
                _player.Pause();
            }
        }

        private FrameworkElement CreateMediaContent(string path)
        {
            Debug.WriteLine($"Create.MediaPlayer: {ArchiveEntry}");

            // TODO: コンテンツ再生成される原因は？
            // TODO: Player が１つだけであることを保証するもっときれいな実装
            if (Debugger.IsAttached)
            {
                Debug.Assert(_player == null);
            }
            if (_player != null)
            {
                CloseMediaPlayer(_player);
                _player = null;
            }


            var videoDrawing = new VideoDrawing()
            {
                //Rect = new Rect(size),
                Rect = new Rect(0, 0, 256, 256), // Stretch.Fill で引き伸ばすので値は適応でも良い
                //Player = _player,
            };

            var brush = new DrawingBrush()
            {
                Drawing = videoDrawing,
                Stretch = Stretch.Fill,
                Viewbox = Element.ViewSizeCalculator.GetViewBox(),
            };

            var rectangle = new Rectangle()
            {
                Fill = brush,
            };
            rectangle.SetBinding(Rectangle.VisibilityProperty, new Binding(nameof(VideoVisibility)) { Source = this });
            //rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode); // 効果なし

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

            var grid = new Grid()
            {
                Background = Brushes.Black,
            };
            grid.Children.Add(rectangle);
            grid.Children.Add(_errorMessageTextBlock);

            // NOTE: コントロールの存在を保証するためにプレイヤー生成は最後に行う
            _player = OpenMediaPlayer(new Uri(path));
            videoDrawing.Player = _player;

            return grid;
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
