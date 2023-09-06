using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class SimpleMediaPlayer : IDisposable, INotifyPropertyChanged
    {
        private MediaPlayer _player;
        private DisposableCollection _disposables = new();
        private bool _disposedValue;
        private MediaArchiveConfig _mediaConfig;
        private TimeSpan _startDelay;
        private bool _isPlaying;
        private bool _resumePlaying;

        public SimpleMediaPlayer(MediaPlayer player)
        {
            _player = player;
            _mediaConfig = Config.Current.Archive.Media;

            _disposables.Add(_mediaConfig.SubscribePropertyChanged(nameof(MediaArchiveConfig.IsMuted),
                (s, e) => IsMuted = _mediaConfig.IsMuted));

            _disposables.Add(_mediaConfig.SubscribePropertyChanged(nameof(MediaArchiveConfig.Volume),
                (s, e) => Volume = _mediaConfig.Volume));
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler? MediaPlayed;


        public event EventHandler<ExceptionEventArgs> MediaFailed
        {
            add => _player.MediaFailed += value;
            remove => _player.MediaFailed -= value;
        }

        public event EventHandler MediaEnded
        {
            add => _player.MediaEnded += value;
            remove => _player.MediaEnded -= value;
        }

        public event EventHandler MediaOpened
        {
            add => _player.MediaOpened += value;
            remove => _player.MediaOpened -= value;
        }


        public MediaPlayer Player => _player;


        // ループ制御
        public bool HasControl { get; set; } = true;

        public bool IsRepeat => _mediaConfig.IsRepeat;

        public bool IsMuted
        {
            get { return _player.IsMuted; }
            set { _player.IsMuted = value && _mediaConfig.IsMuted; }
        }

        public double Volume
        {
            get { return _player.Volume; }
            set { _player.Volume = value; }
        }

        public bool ScrubbingEnabled
        {
            get { return _player.ScrubbingEnabled; }
            set { _player.ScrubbingEnabled = value; }
        }

        public TimeSpan Position
        {
            get { return _player.Position; }
            set { _player.Position = value; }
        }

        public Duration NaturalDuration
        {
            get { return _player.NaturalDuration; }
        }






        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Open(Uri uri, TimeSpan startDelay)
        {
            if (_disposedValue) return;

            _startDelay = startDelay;

            _player.MediaOpened += Player_MediaOpened;
            _player.MediaEnded += Player_MediaEnded;
            _player.MediaFailed += Player_MediaFailed;
            _player.IsMuted = true;
            _player.Volume = _mediaConfig.Volume;

            _player.Open(uri);
            _player.Play();
            _isPlaying = true;
            _resumePlaying = false;

            // NOTE: MP3等、映像がない場合はOpenedイベントが発生しないため、すぐに再生する。
            if (_player.HasAudio && !_player.HasVideo)
            {
                OnStarted();
            }
        }

        public void Play()
        {
            if (_disposedValue) return;

            _player.Play();
            _isPlaying = true;
            _resumePlaying = false;
        }

        public void Stop()
        {
            if (_disposedValue) return;

            _resumePlaying = _isPlaying;
            _player.Stop();
            _isPlaying = false;
        }

        public void Pause()
        {
            if (_disposedValue) return;

            _resumePlaying = _isPlaying;
            _player.Pause();
            _isPlaying = false;
        }

        public void Resume()
        {
            if (_disposedValue) return;

            if (_resumePlaying)
            {
                Play();
            }
        }

        private void OnStarted()
        {
            if (_disposedValue) return;

            IsMuted = false;
            MediaPlayed?.Invoke(this, EventArgs.Empty);
        }

        private void Player_MediaOpened(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(NaturalDuration));
            DelayAction(OnStarted, _startDelay);
        }

        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            if (!HasControl) return;

            if (_mediaConfig.IsRepeat)
            {
                _player.Position = TimeSpan.FromMilliseconds(1);
            }
            else
            {
                _player.Pause();
            }
        }

        private void Player_MediaFailed(object? sender, ExceptionEventArgs e)
        {
            // nop.
        }

        private void Close()
        {
            if (_disposedValue) return;

            _player.MediaOpened -= Player_MediaOpened;
            _player.MediaEnded -= Player_MediaEnded;
            _player.MediaFailed -= Player_MediaFailed;
            _player.Stop();
            _player.Close();
            _isPlaying = false;
            _resumePlaying = false;

#if false
            // NOTE: 一瞬黒い画像が表示されるのを防ぐために開放タイミングをずらす。今作では不要か？
            DelayAction(() =>
            {
                _player.Close();
                _mediaPlayerPool.Release(_player);
            }, 16);
#endif
        }


        // TODO: 汎用化？
        private static void DelayAction(Action action, TimeSpan span)
        {
            AppDispatcher.BeginInvoke(async () =>
            {
                await Task.Delay(span);
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
