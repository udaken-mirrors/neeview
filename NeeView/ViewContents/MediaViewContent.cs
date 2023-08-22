using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.Collections.Generic;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;


namespace NeeView
{
    public static class MediaPlayerPool
    {
        // TODO: static ダメ！Book切り替えで開放されるように！
        public static ObjectPool<MediaPlayer> Default { get; } = new();
    }


    public partial class MediaViewContent : ViewContent
    {
        private ObjectPool<MediaPlayer> _mediaPlayerPool;

        private MediaPlayerCanvas? _player;
        private DisposableCollection _disposables = new();
        private bool _disposedValue;


        public MediaViewContent(PageFrameElement element, PageFrameElementScale scale, ViewSource viewSource, PageFrameActivity activity)
            : base(element, scale, viewSource, activity)
        {
            _mediaPlayerPool = MediaPlayerPool.Default;
        }


        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_player is not null)
                    {
                        _player.MediaPlayed -= Player_MediaPlayed;
                        _player.Dispose();
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
            var source = data as MediaSource ?? throw new InvalidOperationException();
            return CreateMediaContent(source);
        }


        private FrameworkElement CreateMediaContent(MediaSource source)
        {
            Debug.WriteLine($"Create.MediaPlayer: {ArchiveEntry}");

            var viewbox = Element.ViewSizeCalculator.GetViewBox();

            if (_player is not null)
            {
                _player.SetViewbox(viewbox);
                return _player;
            }

            _player = new MediaPlayerCanvas(source, viewbox, _mediaPlayerPool);
            _player.MediaPlayed += Player_MediaPlayed;

            return _player;
        }

        private void Player_MediaPlayed(object? sender, EventArgs e)
        {
            UpdateVideoStatus();
        }

        private void UpdateVideoStatus()
        {
            if (_player is null) return;

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
    }

}
