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


    public class MediaViewContentStrategy : IDisposable, IViewContentStrategy, IHasImageSource, IHasMediaPlayer, IHasScalingMode
    {
        private readonly ViewContent _viewContent;

        private readonly ViewContentMediaPlayer _mediaPlayer;
        private readonly ICoreMediaPlayer _player;
        private readonly IMediaContext _mediaContext;
        private MediaPlayerCanvas? _playerCanvas;
        private ImageSource? _imageSource;
        private bool _disposedValue;
        private BitmapScalingMode? _scalingMode;


        public MediaViewContentStrategy(ViewContent viewContent)
        {
            _viewContent = viewContent;

            // メディアブックとメティアページで参照する設定を変える
            _mediaContext = _viewContent.Page.ArchiveEntry.Archiver is MediaArchiver ? Config.Current.Archive.Media : PageMediaContext.Current;


            _player = AllocateMediaPlayer();
            _mediaPlayer = new ViewContentMediaPlayer(_mediaContext, _player, _viewContent.Activity, _viewContent.ElementIndex);
        }


        public ImageSource? ImageSource => _imageSource;

        public IMediaPlayer Player => _mediaPlayer;


        public BitmapScalingMode? ScalingMode
        {
            get { return _scalingMode; }
            set
            {
                if (_scalingMode != value)
                {
                    _scalingMode = value;
                    if (_mediaPlayer is IHasScalingMode hasScalingMode)
                    {
                        hasScalingMode.ScalingMode = _scalingMode;
                    }
                }
            }
        }

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

            _playerCanvas = MediaPlayerCanvasFactory.Create(_viewContent.Element, viewData, _viewContent.ViewContentSize, viewbox, _player);
            _player.Open(new Uri(viewData.Path), TimeSpan.FromSeconds(_mediaContext.MediaStartDelaySeconds));

            return _playerCanvas;
        }


        private ICoreMediaPlayer AllocateMediaPlayer()
        {
            if (Config.Current.Archive.Media.IsLibVlcEnabled && _viewContent.Page.Content is not AnimatedPageContent)
            {
                try
                {
                    if (_viewContent.Page.Content.PictureInfo is PictureInfo pictureInfo)
                    {
                        pictureInfo.Decoder = "libVLC";
                    }
                    return new VlcCoreMediaPlayer();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Cannot use libVLC.\r\n{ex.Message}", ex);
                }
            }
            else
            {
                try
                {
                    if (_viewContent.Page.Content.PictureInfo is PictureInfo pictureInfo)
                    {
                        pictureInfo.Decoder = "MediaPlayer";
                    }
                    return new DefaultCoreMediaPlayer();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Cannot create Media player.\r\n{ex.Message}", ex);

                }
            }
        }

        private void ReleaseMediaPlayer(ICoreMediaPlayer player)
        {
            player.Dispose();
        }
    }
}
