namespace NeeView
{
    public class SvgPageThumbnail : PicturePageThumbnail
    {
        public SvgPageThumbnail(SvgPageContent content) : base(content, new SvgPictureSource(content))
        {
        }
    }
}
