namespace NeeView
{
    public class PdfViewSourceStrategy : ImageViewSourceStrategy
    {
        public PdfViewSourceStrategy(ArchiveEntry archiveEntry, PictureInfo? pictureInfo)
            : base(new PdfPictureSource(archiveEntry, pictureInfo))
        {
        }
    }

}
