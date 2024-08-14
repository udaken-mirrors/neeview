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
        public double Width
        {
            get
            {
                if (this.Source.Content is BitmapPageContent bitmapContent && bitmapContent.PictureInfo != null)
                {
                    return bitmapContent.PictureInfo.OriginalSize.Width;
                }
                else
                {
                    return 0.0;
                }
            }
        }

        [WordNodeMember]
        public double Height
        {
            get
            {
                if (this.Source.Content is BitmapPageContent bitmapContent && bitmapContent.PictureInfo != null)
                {
                    return bitmapContent.PictureInfo.OriginalSize.Height;
                }
                else
                {
                    return 0.0;
                }
            }
        }

        [WordNodeMember]
        public MediaPlayerAccessor? Player => _mediaPlayer is not null ? new MediaPlayerAccessor(_mediaPlayer) : null;


        [WordNodeMember]
        public PageAccessor GetPageAccessor() => new PageAccessor(Source);
    }
}
