namespace NeeView
{
    public class SvgViewSourceStrategy : ImageViewSourceStrategy
    {
        public SvgViewSourceStrategy(PageContent pageContent)
            : base(pageContent, new SvgPictureSource(pageContent))
        {
        }
    }

}
