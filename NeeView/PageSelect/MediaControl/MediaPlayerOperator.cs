using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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


        private SimpleMediaPlayer _player;
        private readonly DispatcherTimer _timer;

        private bool _isLastStart;
        private bool _isTimeLeftDisp;

        private Duration _duration;
        private TimeSpan _durationTimeSpan = TimeSpan.FromMilliseconds(1.0);
        private TimeSpan _position;

        private bool _isActive;
        private bool _isPlaying;
        private bool _isRepeat;
        private bool _isScrubbing;
        private double _volume = 0.5;
        private double _delay;
        private readonly DisposableCollection _disposables = new();
        private bool _disposedValue = false;


        public MediaPlayerOperator(SimpleMediaPlayer player)
        {
            _player = player;

            _player.ScrubbingEnabled = true;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;
            _disposables.Add(_player.SubscribePropertyChanged(nameof(_player.NaturalDuration),
                (s, e) => Duration = _player.NaturalDuration));

            _position = _player.Position;

            this.IsMuted = Config.Current.Archive.Media.IsMuted;
            this.Volume = Config.Current.Archive.Media.Volume;
            this.IsRepeat = Config.Current.Archive.Media.IsRepeat;

            _timer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += DispatcherTimer_Tick;
            _timer.Start();

            _disposables.Add(Config.Current.Archive.Media.SubscribePropertyChanged(nameof(MediaArchiveConfig.IsMuted),
                (s, e) =>
                {
                    this.IsMuted = Config.Current.Archive.Media.IsMuted;
                }));

            _disposables.Add(Config.Current.Archive.Media.SubscribePropertyChanged(nameof(MediaArchiveConfig.Volume),
                (s, e) =>
                {
                    this.Volume = Config.Current.Archive.Media.Volume;
                }));

            _disposables.Add(Config.Current.Archive.Media.SubscribePropertyChanged(nameof(MediaArchiveConfig.IsRepeat),
                (s, e) =>
                {
                    this.IsRepeat = Config.Current.Archive.Media.IsRepeat;
                }));
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

        private void SetPositionInner(TimeSpan position)
        {
            //Debug.Assert(position > TimeSpan.Zero);
            _position = MathUtility.Clamp(position, TimeSpan.Zero, _durationTimeSpan);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(PositionRate));
            RaisePropertyChanged(nameof(DispTime));
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
            get { return _volume; }
            set
            {
                if (_disposedValue) return;
                if (_volume != value)
                {
                    _volume = value;
                    UpdateVolume();
                    RaisePropertyChanged();

                    Config.Current.Archive.Media.Volume = _volume;
                }
            }
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
                if (_disposedValue) return;
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsRepeat
        {
            get { return _isRepeat; }
            set
            {
                if (_disposedValue) return;
                if (_isRepeat != value)
                {
                    _isRepeat = value;
                    RaisePropertyChanged();

                    Config.Current.Archive.Media.IsRepeat = _isRepeat;
                }
            }
        }

        public bool IsMuted
        {
            get { return _player.IsMuted; }
            set
            {
                if (_disposedValue) return;
                _player.IsMuted = value;
                RaisePropertyChanged();
                Config.Current.Archive.Media.IsMuted = _player.IsMuted;
            }
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
                            _player.Pause();
                            UpdateVolume();
                        }
                        else
                        {
                            Resume();
                            _player.Position = _position;
                        }
                    }

                    RaisePropertyChanged();
                }
            }
        }







        #region IDisposable Support


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
                    //_player.Stop();
                    //_player.Close();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion



        public void RaisePropertyChangedAll()
        {
            RaisePropertyChanged(null);
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


        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            Dispose();
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (!_isRepeat)
            {
                MediaEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            return;
        }


        // 遅延再生開始用のタイマー処理
        private void DispatcherTimer_StartTick(object? sender, EventArgs e)
        {
            _delay -= _timer.Interval.TotalMilliseconds;
            if (_delay < 0.0)
            {
                _timer.Tick -= DispatcherTimer_StartTick;
                Play();
            }
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

            Delay_Tick(_timer.Interval.TotalMilliseconds);
        }

        public void Attach(bool isPlaying)
        {
            if (_disposedValue) return;

            _isActive = isPlaying;
            _isPlaying = isPlaying;

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

            _player.Play();
            UpdateVolume();

            this.IsPlaying = true;
        }

        public void Pause()
        {
            if (_disposedValue) return;

            _player.Pause();

            IsPlaying = false;
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

            _delay = Config.Current.Archive.Media.MediaStartDelaySeconds * 1000;
            if (_delay <= 0.0)
            {
                this.Position = position;
            }
            else
            {
                UpdateVolume();
                _player.Pause();
                this.Position = position;

                _delayPosition = position;
            }
        }

        TimeSpan _delayPosition;

        // 移動による遅延再生処理用
        private void Delay_Tick(double ms)
        {
            if (_disposedValue) return;
            if (_delay <= 0.0) return;

            if (_isScrubbing)
            {
                _delay = 0.0;
                return;
            }

#warning 無理やり位置補正しているのがよろしくない。この遅延の仕組み自体がいいかげんである。であるが、次の動画再生システムまでのつなぎなのでまあ？

            this.Position = _delayPosition;

            _delay -= ms;
            if (_delay <= 0.0)
            {
                Resume();
            }
        }

        //
        private void Resume()
        {
            if (_disposedValue) return;

            if (_isPlaying && (_isRepeat || _position < _durationTimeSpan))
            {
                _player.Play();
                UpdateVolume();
            }
        }

        //
        private void UpdateVolume()
        {
            _player.Volume = _isScrubbing || _delay > 0.0 ? 0.0 : _volume;
        }

        public void AddVolume(double delta)
        {
            Volume = MathUtility.Clamp(Volume + delta, 0.0, 1.0);
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
