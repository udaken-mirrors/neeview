namespace NeeView
{
    [DocumentableBaseClass(typeof(PageAccessor))]
    public record class ViewPageAccessor : PageAccessor
    {
        private readonly IMediaPlayer? _mediaPlayer;

        public ViewPageAccessor(Page page, IMediaPlayer? mediaPlayer) : base(page)
        {
            _mediaPlayer = mediaPlayer;
        }

        [WordNodeMember]
        public double Width => this.Source.GetContentPictureInfo()?.OriginalSize.Width ?? 0.0;

        [WordNodeMember]
        public double Height => this.Source.GetContentPictureInfo()?.OriginalSize.Height ?? 0.0;

        [WordNodeMember]
        public MediaPlayerAccessor? Player => _mediaPlayer is not null ? new MediaPlayerAccessor(_mediaPlayer) : null;


        [WordNodeMember]
        public PageAccessor GetPageAccessor() => new PageAccessor(Source);
    }
}
