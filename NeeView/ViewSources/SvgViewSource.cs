namespace NeeView
{
    public class SvgViewSource : PictureViewSource
    {
        public SvgViewSource(SvgPageContent pageContent, BookMemoryService bookMemoryService) : base(pageContent,  new SvgPictureSource(pageContent), bookMemoryService)
        {
        }
    }
}