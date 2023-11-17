namespace NeeView
{
    public class BitmapPageThumbnail : ImagePageThumbnail
    {
        public BitmapPageThumbnail(BitmapPageContent content) : base(content, new BitmapPictureSource(content.ArchiveEntry, content.PictureInfo))
        {
        }
    }
}
