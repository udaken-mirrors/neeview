using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Vlc.DotNet.Core;
using Vlc.DotNet.Core.Interops;
using Vlc.DotNet.Core.Interops.Signatures;
using Vlc.DotNet.Wpf;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class VlcCoreMediaPlayer : ICoreMediaPlayer, IDisposable
    {
        private static object _lock = new object();

        private readonly VlcVideoSourceProvider _source;
        private readonly VlcMediaPlayer _player;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private bool _isEnabled = true;
        private bool _isMuted;
        private bool _isRepeat;
        private bool _isPlaying;
        private bool _isEnded;
        private bool _isOpened;
        private bool _hasAudio;
        private bool _hasVideo;
        private Duration _duration;
        private VlcTrackCollectionSource? _audioTracks;
        private VlcTrackCollectionSource? _subtitles;

        private Uri _uri = new Uri(".", UriKind.Relative);
        private string _option = "";


        public VlcCoreMediaPlayer()
        {
            lock (_lock)
            {
                var libDirectory = new DirectoryInfo(Config.Current.Archive.Media.LibVlcPath);
                if (!libDirectory.Exists) throw new DirectoryNotFoundException($"The directory containing libvlc.dll does not exist: {libDirectory.FullName}");
                _source = new VlcVideoSourceProvider(Application.Current.Dispatcher);
                _source.CreatePlayer(libDirectory);
                _player = _source.MediaPlayer;
            }

            _player.EndReached += Player_EndReached;
            _player.EncounteredError += Player_EncounteredError;

            //_player.TimeChanged +=
            //    (s, e) => RaisePropertyChanged(nameof(Position));
            _player.AudioVolume +=
                (s, e) => RaisePropertyChanged(nameof(Volume));
            _player.SeekableChanged +=
                (s, e) => RaisePropertyChanged(nameof(ScrubbingEnabled));

            _disposables.Add(this.SubscribePropertyChanged(nameof(IsEnabled),
                (s, e) => { UpdatePlayed(); UpdateMuted(); }));
            _disposables.Add(this.SubscribePropertyChanged(nameof(IsMuted),
                (s, e) => UpdateMuted()));
            _disposables.Add(this.SubscribePropertyChanged(nameof(IsRepeat),
                (s, e) => UpdateRepeat()));
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;
        public event EventHandler? MediaPlayed;
        public event EventHandler<ExceptionEventArgs>? MediaFailed;


        public VlcVideoSourceProvider SourceProvider => _source;

        public bool HasAudio
        {
            get { return _hasAudio; }
            private set { SetProperty(ref _hasAudio, value); }
        }


        public bool HasVideo
        {
            get { return _hasVideo; }
            private set { SetProperty(ref _hasVideo, value); }
        }



        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        public bool IsMuted
        {
            get { return _isMuted; }
            set { SetProperty(ref _isMuted, value); }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set { SetProperty(ref _isRepeat, value); }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set { SetProperty(ref _isPlaying, value); }
        }

        public bool ScrubbingEnabled
        {
            get => _player.IsSeekable;
        }


        public Duration Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }


        public double Position
        {
            get
            {
                if (_disposedValue) return 0.0;
                return _player.Position;
            }
            set
            {
                if (_disposedValue) return;
                var newPosition = (float)value;
                if (_player.Position != newPosition)
                {
                    _player.Position = newPosition;
                }
            }
        }

        public double Volume
        {
            get { return _player.Audio.Volume / 100.0; }
            set
            {
                var newVolume = (int)(value * 100.0);
                if (_player.Audio.Volume != newVolume)
                {
                    _player.Audio.Volume = newVolume;
                }
            }
        }

        /// <summary>
        /// オーディオトラック、字幕の選択有効
        /// </summary>
        public bool CanControlTracks => true;

        /// <summary>
        /// オーディオトラックの選択管理
        /// </summary>
        /// <remarks>
        /// NOTE: 取得のたびに生成されます。
        /// </remarks>
        public TrackCollection? AudioTracks
        {
            get
            {
                _audioTracks?.Dispose();
                _audioTracks = null;
                var tracks = _player.Audio.Tracks;
                if (tracks is null || tracks.Count <= 0) return null;
                _audioTracks = new VlcTrackCollectionSource(tracks);
                return _audioTracks.Collection;
            }
        }

        /// <summary>
        /// 字幕の選択管理
        /// </summary>
        /// <remarks>
        /// NOTE: 取得のたびに生成されます。
        /// </remarks>
        public TrackCollection? Subtitles
        {
            get
            {
                _subtitles?.Dispose();
                _subtitles = null;
                var tracks = _player.SubTitles;
                if (tracks is null || tracks.Count <= 0) return null;
                _subtitles = new VlcTrackCollectionSource(tracks);
                return _subtitles.Collection;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    lock (_lock)
                    {
                        _audioTracks?.Dispose();
                        _subtitles?.Dispose();
                        _disposables.Dispose();
                        Task.Run(() =>
                        {
                            try
                            {
                                Debug.WriteLine($"<<<< VlcMediaPlayer.Dispose: {System.Environment.TickCount} {_uri}");
                                _source.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                            }
                        });
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


        public void Open(Uri uri, TimeSpan _)
        {
            if (_disposedValue) return;

            _uri = uri;
            UpdateRepeat();

            _player.Buffering += Player_Buffering;
            lock (_lock)
            {
                Debug.WriteLine($">>>>  VlcMediaPlayer.Open: {System.Environment.TickCount} {_uri}");
                _player.Play(_uri, _option);
            }
            _player.Audio.IsMute = true;
            IsPlaying = true;
        }


        private void SetOption(string option)
        {
            if (_option == option) return;
            _option = option;

            if (_player.IsPlaying())
            {
                lock (_lock)
                {
                    var position = _player.Position;
                    _player.Play(_uri, _option);
                    _player.Position = position;
                }
            }
        }

        private void Player_Buffering(object? sender, VlcMediaPlayerBufferingEventArgs e)
        {
            //Debug.WriteLine($"Buffering: {e.NewCache}");
            if (e.NewCache < 100.0f) return;

            _player.Buffering -= Player_Buffering;
            OnStarted();
        }

        private void OnStarted()
        {
            if (_disposedValue) return;

            _isOpened = true;

            UpdatePlayed();
            UpdateMuted();
            UpdateTrackInfo();

            AppDispatcher.BeginInvoke(() => MediaPlayed?.Invoke(this, EventArgs.Empty));
        }

        public void Play()
        {
            if (_disposedValue) return;

            IsPlaying = true;
            UpdatePlayed();

            if (_isEnded && _isRepeat)
            {
                Replay();
            }
        }

        public void Pause()
        {
            if (_disposedValue) return;

            IsPlaying = false;
            UpdatePlayed();
        }

        private void Replay()
        {
            _isEnded = false;
            lock (_lock)
            {
                _player.Play(_uri, _option);
            }
        }

        private void UpdateTrackInfo()
        {
            var media = _player.GetMedia();
            HasAudio = media.Tracks.Any(e => e.Type == MediaTrackTypes.Audio);
            HasVideo = media.Tracks.Any(e => e.Type == MediaTrackTypes.Video);
            Duration = new Duration(TimeSpan.FromMilliseconds(_player.Length));
        }

        private void UpdatePlayed()
        {
            var isPlay = _isEnabled && _isPlaying;

            lock (_lock)
            {
                if (isPlay)
                {
                    _player.Play();
                }
                else
                {
                    _player.Pause();
                }
            }
        }

        private void UpdateMuted()
        {
            var isMuted = !_isEnabled || !_isOpened || _isMuted;
            _player.Audio.IsMute = isMuted;
        }

        private void UpdateRepeat()
        {
            SetOption(IsRepeat ? "input-repeat=65535" : "");

            if (_isRepeat && _isEnded && _isPlaying)
            {
                Replay();
            }
        }

        private void Player_EndReached(object? sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            if (_isRepeat)
            {
                Task.Run(Replay);
            }
            else
            {
                _isEnded = true;
            }
            AppDispatcher.BeginInvoke(() => MediaEnded?.Invoke(this, e));
        }

        private void Player_EncounteredError(object? sender, VlcMediaPlayerEncounteredErrorEventArgs e)
        {
            AppDispatcher.BeginInvoke(() => MediaFailed?.Invoke(this, new ExceptionEventArgs(new ApplicationException("libVLC Failed"))));
        }
    }
}

