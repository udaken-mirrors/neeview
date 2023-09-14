using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeView.PageFrames;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;


namespace NeeView
{
    /// <summary>
    /// MediaContext に依存した MediaPlayer
    /// </summary>
    [NotifyPropertyChanged]
    public partial class ViewContentMediaPlayer : IMediaPlayer, INotifyPropertyChanged, IDisposable
    {
        private readonly IMediaContext _mediaContext;
        private readonly SimpleMediaPlayer _player;
        private readonly PageFrameActivity _activity;
        private readonly int _elementIndex;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();

        public ViewContentMediaPlayer(IMediaContext mediaContext, SimpleMediaPlayer player, PageFrameActivity activity, int elementIndex)
        {
            _mediaContext = mediaContext;
            _player = player;
            _activity = activity;
            _elementIndex = elementIndex;

            _disposables.Add(_mediaContext.SubscribePropertyChanged(nameof(IMediaContext.Volume),
                (s, e) => { RaisePropertyChanged(nameof(Volume)); Update(); }));

            _disposables.Add(_mediaContext.SubscribePropertyChanged(nameof(IMediaContext.IsMuted),
                (s, e) => { RaisePropertyChanged(nameof(IsMuted)); Update(); }));

            _disposables.Add(_mediaContext.SubscribePropertyChanged(nameof(IMediaContext.IsRepeat),
                (s, e) => { RaisePropertyChanged(nameof(IsRepeat)); Update(); }));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(SimpleMediaPlayer.IsEnabled),
                (s, e) => RaisePropertyChanged(nameof(IsEnabled))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(SimpleMediaPlayer.HasAudio),
                (s, e) => RaisePropertyChanged(nameof(HasAudio))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(SimpleMediaPlayer.IsPlaying),
                (s, e) => RaisePropertyChanged(nameof(IsPlaying))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(SimpleMediaPlayer.HasVideo),
                (s, e) => RaisePropertyChanged(nameof(HasVideo))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(SimpleMediaPlayer.ScrubbingEnabled),
                (s, e) => RaisePropertyChanged(nameof(ScrubbingEnabled))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(SimpleMediaPlayer.NaturalDuration),
                (s, e) => RaisePropertyChanged(nameof(NaturalDuration))));

            _disposables.Add(_player.SubscribePropertyChanged(nameof(SimpleMediaPlayer.Position),
                (s, e) => RaisePropertyChanged(nameof(Position))));

            _disposables.Add(_activity.SubscribePropertyChanged(
                (s, e) => Update()));

            Update();
        }


        public event EventHandler MediaEnded
        {
            add => _player.MediaEnded += value;
            remove => _player.MediaEnded += value;
        }

        public event EventHandler<ExceptionEventArgs> MediaFailed
        {
            add => _player.MediaFailed += value;
            remove => _player.MediaFailed += value;
        }

        public event EventHandler MediaOpened
        {
            add => _player.MediaOpened += value;
            remove => _player.MediaOpened += value;
        }

        public event EventHandler? MediaPlayed
        {
            add => _player.MediaPlayed += value;
            remove => _player.MediaPlayed += value;
        }

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsEnabled
        {
            get => _player.IsEnabled;
            set => _player.IsEnabled = value;
        }

        public bool HasAudio => _player.HasAudio;

        public bool HasVideo => _player.HasVideo;

        public bool IsPlaying => _player.IsPlaying;

        public bool ScrubbingEnabled
        {
            get => _player.ScrubbingEnabled;
            set => _player.ScrubbingEnabled = value;
        }

        public Duration NaturalDuration => _player.NaturalDuration;

        public TimeSpan Position
        {
            get => _player.Position;
            set => _player.Position = value;
        }

        public bool IsMuted
        {
            get => _mediaContext.IsMuted;
            set => _mediaContext.IsMuted = value;
        }

        public double Volume
        {
            get => _mediaContext.Volume;
            set => _mediaContext.Volume = value;
        }

        public bool IsRepeat
        {
            get => _mediaContext.IsRepeat;
            set => _mediaContext.IsRepeat = value;
        }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
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

        public void Play()
        {
            if (_disposedValue) return;
            _player.Play();
        }

        public void Pause()
        {
            if (_disposedValue) return;
            _player.Pause();
        }

        private void Update()
        {
            if (_disposedValue) return;
            
            //Debug.WriteLine($"Media.UpdateState: {Page}");
            _player.IsEnabled = _activity.IsVisible;
            _player.IsMuted = !_activity.IsSelected || _elementIndex != 0 || _mediaContext.IsMuted;
            _player.Volume = _mediaContext.Volume;
            _player.IsRepeat = _mediaContext.IsRepeat;
        }

    }

}
