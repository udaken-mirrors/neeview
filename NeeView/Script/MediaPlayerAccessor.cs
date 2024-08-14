namespace NeeView
{
    public record class MediaPlayerAccessor
    {
        private readonly IMediaPlayer _player;

        public MediaPlayerAccessor(IMediaPlayer player)
        {
            _player = player;
        }

        [WordNodeMember]
        public double Duration
        {
            get => _player.Duration.TimeSpan.TotalSeconds;
        }

        [WordNodeMember]
        public TrackCollectionAccessor? AudioTrack
        {
            get => _player.AudioTracks is not null ? new TrackCollectionAccessor(_player.AudioTracks) : null;
        }

        [WordNodeMember]
        public TrackCollectionAccessor? Subtitle
        {
            get => _player.Subtitles is not null ? new TrackCollectionAccessor(_player.Subtitles) : null;
        }

        [WordNodeMember]
        public double Volume
        {
            get => Config.Current.Archive.Media.Volume;
            set => Config.Current.Archive.Media.Volume = value;
        }

        [WordNodeMember]
        public bool IsMuted
        {
            get => Config.Current.Archive.Media.IsMuted;
            set => Config.Current.Archive.Media.IsMuted = value;
        }

        [WordNodeMember]
        public bool IsRepeat
        {
            get => _player.IsRepeat;
            set => AppDispatcher.BeginInvoke(() => _player.IsRepeat = value);
        }

        [WordNodeMember]
        public double Position
        {
            get => _player.Position;
            set => AppDispatcher.BeginInvoke(() => _player.Position = value);
        }

        [WordNodeMember]
        public bool IsPlaying
        {
            get => _player.IsPlaying;
            set => AppDispatcher.BeginInvoke(() =>
            {
                if (value)
                {
                    _player.Play();
                }
                else
                {
                    _player.Pause();
                }
            });
        }
    }
}
