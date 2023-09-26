//#define DUMP_VLC_EVENT

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
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
    public partial class VlcMediaPlayer : IOpenableMediaPlayer, IDisposable
    {
        private static readonly object _lock = new();

        private readonly VlcVideoSourceProvider _source;
        private readonly Vlc.DotNet.Core.VlcMediaPlayer _player;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private bool _isEnabled = true;
        private bool _isMuted;
        private bool _isRepeat;
        private bool _isPlaying;
        private bool _isOpened;
        private bool _hasAudio;
        private bool _hasVideo;
        private bool _scrubbingEnabled;
        private Duration _duration;
        private VlcTrackCollectionSource? _audioTracks;
        private VlcTrackCollectionSource? _subtitles;
        private Uri? _uri;

        /// <summary>
        /// 再生位置要求
        /// </summary>
        /// <remarks>
        /// 再生終了後の位置要求を処理するためのもの。リピートの切り替え処理が存在しないため、対応するために複雑な処理になっている。
        /// 再生停止後の再再生は位置が０になってしまい、設定タイミングによってはそれも無効化されてしまうため、この変数で位置を要求する。
        /// - NegativeInfinity で要求なし
        /// - Playing イベントで要求の位置に設定
        /// - 外部からの Position 設定で要求解除
        /// - PositionChanged イベントで要求位置より進んでいれば要求解除
        /// - 外部への Position はこの要求位置を加味した位置を渡す
        /// </remarks>
        private float _requestPosition = float.NegativeInfinity;


        public VlcMediaPlayer()
        {
            lock (_lock)
            {
                var libDirectory = new DirectoryInfo(Config.Current.Archive.Media.LibVlcPath);
                if (!libDirectory.Exists) throw new DirectoryNotFoundException($"The directory containing libvlc.dll does not exist: {libDirectory.FullName}");
                _source = new VlcVideoSourceProvider(Application.Current.Dispatcher);
                _source.CreatePlayer(libDirectory);
                _player = _source.MediaPlayer;
            }

            AttachPlayer();


#if DUMP_VLC_EVENT
            _player.MediaChanged += (s, e) => Trace($"MediaChanged: {e.NewMedia}");
            _player.Opening += (s, e) => Trace($"Opening: ");
            _player.Buffering += (s, e) => Trace($"Buffering: {e.NewCache}");
            _player.Playing += (s, e) => Trace($"Playing: ");
            _player.Paused += (s, e) => Trace($"Paused: ");
            _player.EndReached += (s, e) => Trace($"EndReached: ");
            _player.EncounteredError += (s, e) => Trace($"EncounteredError: ");
            _player.Corked += (s, e) => Trace($"Corked: ");
            _player.AudioDevice += (s, e) => Trace($"AudioDevice: {e.Device}");
            _player.Muted += (s, e) => Trace($"Muted: ");
            _player.AudioVolume += (s, e) => Trace($"AudioVolume: ");
            _player.ChapterChanged += (s, e) => Trace($"ChapterChanged: ");
            _player.Forward += (s, e) => Trace($"Forward: ");
            _player.Backward += (s, e) => Trace($"Backward: ");
            _player.EsAdded += (s, e) => Trace($"EsAdded: {e.Id}");
            _player.EsDeleted += (s, e) => Trace($"EsDeleted: {e.Id}");
            _player.EsSelected += (s, e) => Trace($"EsSelected: {e.Id}");
            _player.LengthChanged += (s, e) => Trace($"LengthChanged: {e.NewLength}");
            _player.TimeChanged += (s, e) => Trace($"TimeChanged: {e.NewTime}");
            _player.TitleChanged += (s, e) => Trace($"TitleChanged: {e.NewTitle}");
            _player.PausableChanged += (s, e) => Trace($"PausableChanged: {e.IsPaused}");
            _player.PositionChanged += (s, e) => Trace($"PositionChanged: {e.NewPosition}");
            _player.ScrambledChanged += (s, e) => Trace($"ScrambledChanged: {e.NewScrambled}");
            _player.SeekableChanged += (s, e) => Trace($"SeekableChanged: {e.NewSeekable}");
            _player.SnapshotTaken += (s, e) => Trace($"SnapshotTaken: {e.FileName}");
            _player.Stopped += (s, e) => Trace($"Stopped:");
            _player.Uncorked += (s, e) => Trace($"Uncorked:");
            _player.Unmuted += (s, e) => Trace($"Unmuted:");
            _player.VideoOutChanged += (s, e) => Trace($"VideoOutChanged: {e.NewCount}");
            //_player.Log += (s, e) => Trace($"Log: {e.Message} ");
#endif
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
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    UpdatePlayed();
                    UpdateMuted();
                }
            }
        }

        public bool IsMuted
        {
            get { return _isMuted; }
            set
            {
                if (SetProperty(ref _isMuted, value))
                {
                    UpdateMuted();
                }
            }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set
            {
                if (SetProperty(ref _isRepeat, value))
                {
                    UpdateRepeat();
                }
            }
        }

        public bool IsPlaying
        {
            get { return _isPlaying; }
            private set { SetProperty(ref _isPlaying, value); }
        }

        public bool ScrubbingEnabled
        {
            get { return _scrubbingEnabled; }
            private set { SetProperty(ref _scrubbingEnabled, value); }
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
                return _player.State == MediaStates.Ended ? 1.0 : Math.Max(_player.Position, _requestPosition);
            }
            set
            {
                if (_disposedValue) return;
                var newPosition = (float)value;
                if (_player.Position != newPosition)
                {
                    Trace($"Position = {newPosition}");
                    _requestPosition = float.NegativeInfinity;
                    _player.Position = newPosition;
                    if (_player.State == MediaStates.Ended)
                    {
                        PlayStart(newPosition);
                    }
                }
            }
        }

        public double Volume
        {
            get
            {
                if (_disposedValue) return 0.0;
                return _player.Audio.Volume / 100.0;
            }
            set
            {
                if (_disposedValue) return;
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
                if (_disposedValue) return null;
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
                if (_disposedValue) return null;
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
                        DetachPlayer();
                        _audioTracks?.Dispose();
                        _subtitles?.Dispose();
                        _disposables.Dispose();
                        Task.Run(() =>
                        {
                            try
                            {
                                //Trace($"VlcMediaPlayer.Dispose: {System.Environment.TickCount} {_uri}");
                                _source.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex);
                                Debugger.Break();
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


        private void AttachPlayer()
        {
            _player.Playing += Player_Playing;
            _player.EndReached += Player_EndReached;
            _player.EncounteredError += Player_EncounteredError;
            _player.PositionChanged += Player_PositionChanged;
            _player.AudioVolume += Player_AudioVolume;
            _player.SeekableChanged += Player_SeekableChanged;
        }

        private void DetachPlayer()
        {
            _player.Playing -= Player_Playing;
            _player.EndReached -= Player_EndReached;
            _player.EncounteredError -= Player_EncounteredError;
            _player.PositionChanged -= Player_PositionChanged;
            _player.AudioVolume -= Player_AudioVolume;
            _player.SeekableChanged -= Player_SeekableChanged;
        }


        public void Open(Uri uri, TimeSpan _)
        {
            if (_disposedValue) return;

            _uri = uri;

            _player.Playing += Player_FirstPlaying;

            lock (_lock)
            {
                //Trace($"VlcMediaPlayer.Open: {System.Environment.TickCount} {_uri}");
                PlayStart();
            }
            _player.Audio.IsMute = true;
            IsPlaying = true;
            ScrubbingEnabled = _player.IsSeekable;

            void Player_FirstPlaying(object? sender, VlcMediaPlayerPlayingEventArgs e)
            {
                _player.Playing -= Player_FirstPlaying;
                OnStarted();
            }
        }


        private void Player_Playing(object? sender, VlcMediaPlayerPlayingEventArgs e)
        {
            Task.Run(() =>
            {
                Trace($"Playing: {_player.Position} => {_requestPosition}");
                UpdatePlayed();

                if (0.0 <= _requestPosition)
                {
                    _player.Position = _requestPosition;
                }
            });
        }
        private void Player_EndReached(object? sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            RaisePropertyChanged(nameof(Position));
            AppDispatcher.BeginInvoke(() => MediaEnded?.Invoke(this, e));
        }

        private void Player_EncounteredError(object? sender, VlcMediaPlayerEncounteredErrorEventArgs e)
        {
            AppDispatcher.BeginInvoke(() => MediaFailed?.Invoke(this, new ExceptionEventArgs(new ApplicationException("libVLC Failed"))));
        }

        private void Player_PositionChanged(object? sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            if (0.0 <= _requestPosition && _requestPosition <= e.NewPosition)
            {
                //Trace($"RequestPosition.Reset");
                _requestPosition = float.NegativeInfinity;
            }
        }

        private void Player_AudioVolume(object? sender, VlcMediaPlayerAudioVolumeEventArgs e)
        {
            RaisePropertyChanged(nameof(Volume));
        }

        private void Player_SeekableChanged(object? sender, VlcMediaPlayerSeekableChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ScrubbingEnabled));
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
        }

        public void Pause()
        {
            if (_disposedValue) return;

            IsPlaying = false;
            UpdatePlayed();
        }

        private void UpdateTrackInfo()
        {
            if (_disposedValue) return;

            var media = _player.GetMedia();
            HasAudio = media.Tracks.Any(e => e.Type == MediaTrackTypes.Audio);
            HasVideo = media.Tracks.Any(e => e.Type == MediaTrackTypes.Video);
            Duration = new Duration(TimeSpan.FromMilliseconds(_player.Length));
        }

        private void UpdatePlayed()
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                if (ShouldPlay())
                {
                    if (_player.State == MediaStates.Ended)
                    {
                        PlayStart(true);
                    }
                    else
                    {
                        _player.Play();
                    }
                }
                else
                {
                    _player.SetPause(true);
                }
            }
        }

        private void UpdateMuted()
        {
            if (_disposedValue) return;

            var isMuted = !_isEnabled || !_isOpened || _isMuted;
            _player.Audio.IsMute = isMuted;
        }

        private void UpdateRepeat()
        {
            if (_disposedValue) return;

            if (!_isOpened) return;

            var keepPosition = _player.State != MediaStates.Ended;
            PlayStart(keepPosition);
        }

        private bool ShouldPlay()
        {
            return _isEnabled && _isPlaying;
        }

        private void PlayStart(bool keepPosition)
        {
            if (keepPosition)
            {
                _requestPosition = _player.Position;
            }
            PlayStart();
        }

        private void PlayStart(double position)
        {
            _requestPosition = (float)position;
            PlayStart();
        }

        private void PlayStart()
        {
            if (_disposedValue) return;

            var options = new List<string>();
            if (_isRepeat)
            {
                options.Add("input-repeat=65535");
            }

            if (_uri is null) return;
            _player.Play(_uri, options.ToArray());
        }

        [Conditional("DEBUG")]
        private void Trace(string message)
        {
            Debug.WriteLine($"VLC: {_player.State}: {message}");
        }
    }
}
