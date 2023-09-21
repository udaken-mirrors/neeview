using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Threading;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MediaPlayer操作
    /// </summary>
    public class MediaPlayerOperator : BindableBase, IDisposable
    {
        /// <summary>
        /// 現在有効なMediaPlayerOperator。
        /// シングルトンではない。
        /// </summary>
        public static MediaPlayerOperator? Current { get; set; }


        private readonly IMediaPlayer _player;
        private readonly DispatcherTimer _timer;
        private bool _isTimeLeftDisp;
        private Duration _duration;
        private TimeSpan _durationTimeSpan = TimeSpan.FromMilliseconds(1.0);
        private TimeSpan _position;
        private bool _isActive;
        private bool _isScrubbing;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue = false;
        private readonly DelayAction _delayResume = new();
        private MediaPlayerPauseBit _pauseBits;
        private readonly object _lock = new();
        private bool _isPlaying;


        public MediaPlayerOperator(IMediaPlayer player)
        {
            _player = player;
            //_player.ScrubbingEnabled = true;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.NaturalDuration),
                (s, e) => Duration = _player.NaturalDuration));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.HasAudio),
                (s, e) => RaisePropertyChanged(nameof(HasAudio))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.HasVideo),
                (s, e) => RaisePropertyChanged(nameof(HasVideo))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.IsPlaying),
                (s, e) => RaisePropertyChanged(nameof(IsPlaying))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.IsMuted),
                (s, e) => RaisePropertyChanged(nameof(IsMuted))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.Volume),
                (s, e) => RaisePropertyChanged(nameof(Volume))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.IsRepeat),
                (s, e) => RaisePropertyChanged(nameof(IsRepeat))));

            _isPlaying = _player.IsPlaying;
            _position = _player.Position;

            _timer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += DispatcherTimer_Tick;
            _timer.Start();
        }


        /// <summary>
        /// 再生が終端に達したときのイベント
        /// </summary>
        public event EventHandler? MediaEnded;


        public Duration Duration
        {
            get { return _duration; }
            set
            {
                if (_disposedValue) return;
                if (_duration != value)
                {
                    _duration = value;
                    _durationTimeSpan = MathUtility.Max(_duration.HasTimeSpan ? _duration.TimeSpan : TimeSpan.Zero, TimeSpan.FromMilliseconds(1.0));
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DurationHasTimeSpan));
                }
            }
        }

        public bool DurationHasTimeSpan
        {
            get { return _duration.HasTimeSpan; }
        }

        public bool HasAudio
        {
            get { return _player.HasAudio; }
        }

        public bool HasVideo
        {
            get { return _player.HasVideo; }
        }

        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                if (_disposedValue) return;
                if (_position != value)
                {
                    SetPositionInner(value);
                    if (_duration.HasTimeSpan)
                    {
                        _player.Position = _position;
                    }
                }
            }
        }


        // [0..1] for slider
        public double PositionRate
        {
            get { return (double)_position.Ticks / _durationTimeSpan.Ticks; }
            set { this.Position = TimeSpan.FromTicks((long)(_durationTimeSpan.Ticks * value)); }
        }

        // [0..1]
        public double Volume
        {
            get => _player.Volume;
            set => _player.Volume = value;
        }

        public bool IsTimeLeftDisp
        {
            get { return _isTimeLeftDisp; }
            set
            {
                if (_disposedValue) return;
                if (_isTimeLeftDisp != value)
                {
                    _isTimeLeftDisp = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DispTime));
                }
            }
        }

        public string? DispTime
        {
            get
            {
                if (!_duration.HasTimeSpan) return null;

                var now = _position;
                var total = _durationTimeSpan;
                var left = total - now;

                var totalString = total.GetHours() > 0 ? $"{total.GetHours()}:{total.Minutes:00}:{total.Seconds:00}" : $"{total.Minutes}:{total.Seconds:00}";

                var nowString = _isTimeLeftDisp
                    ? left.GetHours() > 0 ? $"-{left.GetHours()}:{left.Minutes:00}:{left.Seconds:00}" : $"-{left.Minutes}:{left.Seconds:00}"
                    : now.GetHours() > 0 ? $"{now.GetHours()}:{now.Minutes:00}:{now.Seconds:00}" : $"{now.Minutes}:{now.Seconds:00}";

                return nowString + " / " + totalString;
            }
        }


        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (SetProperty(ref _isPlaying, value))
                {
                    UpdatePlaying();
                }
            }
        }

        public bool IsRepeat
        {
            get => _player.IsRepeat;
            set => _player.IsRepeat = value;
        }

        public bool IsMuted
        {
            get => _player.IsMuted;
            set => _player.IsMuted = value;
        }

        public bool IsScrubbing
        {
            get { return _isScrubbing; }
            set
            {
                if (_disposedValue) return;
                if (_isScrubbing != value)
                {
                    _isScrubbing = value;
                    if (_isActive)
                    {
                        if (_isScrubbing)
                        {
                            SetPauseFlag(MediaPlayerPauseBit.Scrubbing);
                        }
                        else
                        {
                            _player.Position = _position;
                            ResetPauseFlag(MediaPlayerPauseBit.Scrubbing);
                        }
                    }
                    RaisePropertyChanged();
                }
            }
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    MediaEnded = null;
                    _disposables.Dispose();
                    _timer.Stop();
                    _player.MediaFailed -= Player_MediaFailed;
                    _player.MediaOpened -= Player_MediaOpened;
                    _player.MediaEnded -= Player_MediaEnded;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        //public void RaisePropertyChangedAll()
        //{
        //    RaisePropertyChanged(null);
        //}


        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            Dispose();
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (!IsRepeat)
            {
                MediaEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
        }


        // 通常用タイマー処理
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (_disposedValue) return;
            if (!_isActive || _isScrubbing) return;

            if (_duration.HasTimeSpan)
            {
                //Debug.WriteLine($"## Player: {_player.Position}");
                SetPositionInner(_player.Position);
            }
        }

        private void SetPositionInner(TimeSpan position)
        {
            //Debug.Assert(position > TimeSpan.Zero);
            _position = MathUtility.Clamp(position, TimeSpan.Zero, _durationTimeSpan);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(PositionRate));
            RaisePropertyChanged(nameof(DispTime));
        }

        public void Attach()
        {
            if (_disposedValue) return;
            _isActive = true;
            Duration = _player.NaturalDuration;
        }

#if false
        private MediaState GetMediaState(MediaElement myMedia)
        {
            FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
            object helperObject = hlp.GetValue(myMedia);
            FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            MediaState state = (MediaState)stateField.GetValue(helperObject);
            return state;
        }
#endif

        public void Play()
        {
            if (_disposedValue) return;

            _isActive = true;
            IsPlaying = true;
        }

        public void Pause()
        {
            if (_disposedValue) return;

            IsPlaying = false;
        }

        public void TogglePlay()
        {
            if (!IsPlaying)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

        private void SetPauseFlag(MediaPlayerPauseBit bit)
        {
            lock (_lock)
            {
                _pauseBits |= bit;
            }
            UpdatePlaying();
        }

        private void ResetPauseFlag(MediaPlayerPauseBit bit)
        {
            lock (_lock)
            {
                _pauseBits &= ~bit;
            }
            UpdatePlaying();
        }

        /// <summary>
        /// 再生状態更新
        /// </summary>
        private void UpdatePlaying()
        {
            if (_isPlaying && _pauseBits == MediaPlayerPauseBit.None)
            {
                _player.Play();
            }
            else
            {
                _player.Pause();
            }
        }

        /// <summary>
        /// コマンドによる移動
        /// </summary>
        /// <param name="delta"></param>
        /// <returns>終端を超える場合は true</returns>
        public bool AddPosition(TimeSpan delta)
        {
            if (_disposedValue) return false;
            if (!_duration.HasTimeSpan) return false;

            var t0 = _position;
            var t1 = _position + delta;

            if (delta < TimeSpan.Zero && t1 < TimeSpan.Zero && t0 < TimeSpan.FromSeconds(0.5))
            {
                if (IsRepeat)
                {
                    t1 = _durationTimeSpan + t1;
                    t1 = t1 < TimeSpan.Zero ? TimeSpan.Zero : t1;
                }
                else
                {
                    return true;
                }
            }
            if (delta > TimeSpan.Zero && t1 > _durationTimeSpan)
            {
                if (IsRepeat)
                {
                    t1 = TimeSpan.Zero;
                }
                else
                {
                    return true;
                }
            }

            SetPosition(t1);

            return false;
        }

        public void SetPositionFirst()
        {
            SetPosition(TimeSpan.Zero);
        }

        public void SetPositionLast()
        {
            SetPosition(_durationTimeSpan);
        }

        // コマンドによる移動[0..1]
        public void SetPosition(TimeSpan position)
        {
            if (_disposedValue) return;
            if (!_duration.HasTimeSpan) return;

            SetPauseFlag(MediaPlayerPauseBit.SetPosition);
            this.Position = position;
            _delayResume.Request(() => ResetPauseFlag(MediaPlayerPauseBit.SetPosition), TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// 音量増減
        /// </summary>
        /// <param name="delta">増減値</param>
        public void AddVolume(double delta)
        {
            Volume = MathUtility.Clamp(Volume + delta, 0.0, 1.0);
        }


        [Flags]
        private enum MediaPlayerPauseBit
        {
            None = 0,
            Scrubbing = (1 << 0),
            SetPosition = (1 << 1),
        }
    }

    public static class TimeSpanExtensions
    {
        public static int GetHours(this TimeSpan timeSpan)
        {
            return Math.Abs(timeSpan.Days * 24 + timeSpan.Hours);
        }
    }



}
