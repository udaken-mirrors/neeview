namespace NeeView
{
    public class PdfViewSourceStrategy : ImageViewSourceStrategy
    {
        public PdfViewSourceStrategy(PageContent pageContent)
            : base(pageContent, new PdfPictureSource(pageContent))
        {
        }
    }

}
