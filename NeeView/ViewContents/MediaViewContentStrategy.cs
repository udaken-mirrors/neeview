using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;


namespace NeeView
{
    public class AnimatedViewContentStrategy : MediaViewContentStrategy
    {
        public AnimatedViewContentStrategy(ViewContent viewContent) : base(viewContent)
        {
        }
    }


    public class MediaViewContentStrategy : IDisposable, IViewContentStrategy, IHasImageSource, IHasMediaPlayer
    {
        private readonly ViewContent _viewContent;

        private readonly ObjectPool<MediaPlayer> _mediaPlayerPool;
        private readonly ViewContentMediaPlayer _mediaPlayer;
        private readonly SimpleMediaPlayer _player;
        private readonly IMediaContext _mediaContext;
        private MediaPlayerCanvas? _playerCanvas;
        private ImageSource? _imageSource;
        private bool _disposedValue;


        public MediaViewContentStrategy(ViewContent viewContent)
        {
            _mediaPlayerPool = MediaPlayerPool.Default;

            _viewContent = viewContent;

            // メディアブックとメティアページで参照する設定を変える
            _mediaContext = _viewContent.Page.Entry.Archiver is MediaArchiver ? Config.Current.Archive.Media : PageMediaContext.Current;


            _player = AllocateMediaPlayer();
            _mediaPlayer = new ViewContentMediaPlayer(_mediaContext, _player, _viewContent.Activity, _viewContent.ElementIndex);
        }


        public ImageSource? ImageSource => _imageSource;

        public IMediaPlayer Player => _mediaPlayer;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _playerCanvas?.Dispose();
                    _playerCanvas = null;

                    _mediaPlayer.Dispose();
                    ReleaseMediaPlayer(_player);
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void OnSourceChanged()
        {
        }

        public FrameworkElement CreateLoadedContent(object data)
        {
            Debug.WriteLine($"Create.MediaPlayer: {_viewContent.ArchiveEntry}");
            var viewData = data as MediaViewData ?? throw new InvalidOperationException();

            _imageSource = viewData.ImageSource;

            var viewbox = _viewContent.Element.ViewSizeCalculator.GetViewBox();

            if (_playerCanvas is not null)
            {
                _playerCanvas.SetViewbox(viewbox);
                return _playerCanvas;
            }

            _playerCanvas = new MediaPlayerCanvas(viewData, viewbox, _player);
            _player.Open(new Uri(viewData.Path), TimeSpan.FromSeconds(_mediaContext.MediaStartDelaySeconds));

            return _playerCanvas;
        }


        private SimpleMediaPlayer AllocateMediaPlayer()
        {
            var player = new SimpleMediaPlayer(_mediaPlayerPool.Allocate());
            return player;
        }

        private void ReleaseMediaPlayer(SimpleMediaPlayer player)
        {
            player.Dispose();
            _mediaPlayerPool.Release(player.Player);
        }
    }


}
