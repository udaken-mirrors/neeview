namespace NeeView
{
    public class BitmapPageThumbnail : PicturePageThumbnail
    {
        public BitmapPageThumbnail(BitmapPageContent content) : base(content, new BitmapPictureSource(content))
        {
        }
    }
}
