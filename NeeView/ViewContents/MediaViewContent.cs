using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using NeeView.PageFrames;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace NeeView
{
    /// <summary>
    /// MediaPlayer Pool
    /// </summary>
    public static class MediaPlayerPool
    {
        public static ObjectPool<MediaPlayer> Default { get; } = new();
    }


    public partial class MediaViewContent : ViewContent
    {
        private readonly ObjectPool<MediaPlayer> _mediaPlayerPool;

        private readonly ViewContentMediaPlayer _mediaPlayer;
        private readonly SimpleMediaPlayer _player;
        private readonly IMediaContext _mediaContext;
        private MediaPlayerCanvas? _playerCanvas;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue;
        private ImageSource? _imageSource;

        public MediaViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity, PageBackgroundSource backgroundSource, int index)
            : base(element, scale, viewSource, activity, backgroundSource, index)
        {
            _mediaPlayerPool = MediaPlayerPool.Default;

            // メディアブックとメティアページで参照する設定を変える
            _mediaContext = Page.Entry.Archiver is MediaArchiver ? Config.Current.Archive.Media : PageMediaContext.Current;
            _player = AllocateMediaPlayer();

            _mediaPlayer = new ViewContentMediaPlayer(_mediaContext, _player, activity, index);
        }


        public IMediaPlayer Player => _mediaPlayer;

        public IMediaContext MediaContext => _mediaContext;

        public ImageSource? ImageSource => _imageSource;


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

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _playerCanvas?.Dispose();
                    _playerCanvas = null;

                    _mediaPlayer.Dispose();
                    ReleaseMediaPlayer(_player);

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
        }

        protected override FrameworkElement CreateLoadedContent(object data)
        {
            var source = data as MediaSource ?? throw new InvalidOperationException();
            return CreateMediaContent(source);
        }


        private FrameworkElement CreateMediaContent(MediaSource source)
        {
            Debug.WriteLine($"Create.MediaPlayer: {ArchiveEntry}");

            _imageSource = source.ImageSource;

            var viewbox = Element.ViewSizeCalculator.GetViewBox();

            if (_playerCanvas is not null)
            {
                _playerCanvas.SetViewbox(viewbox);
                return _playerCanvas;
            }

            _playerCanvas = new MediaPlayerCanvas(source, viewbox, _player);
            _player.Open(new Uri(source.Path), TimeSpan.FromSeconds(_mediaContext.MediaStartDelaySeconds));

            return _playerCanvas;
        }
    }

}
