namespace NeeView
{
    public class BitmapViewSourceStrategy : ImageViewSourceStrategy
    {
        public BitmapViewSourceStrategy(ArchiveEntry archiveEntry, PictureInfo? pictureInfo)
            : base(new BitmapPictureSource(archiveEntry, pictureInfo))
        {
        }
    }

}
