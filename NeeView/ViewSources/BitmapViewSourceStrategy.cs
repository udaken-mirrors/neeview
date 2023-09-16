namespace NeeView
{
    public class BitmapViewSourceStrategy : ImageViewSourceStrategy
    {
        public BitmapViewSourceStrategy(PageContent pageContent)
            : base(pageContent, new BitmapPictureSource(pageContent))
        {
        }
    }

}
