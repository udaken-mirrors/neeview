namespace NeeView
{
    public class SvgViewSourceStrategy : ImageViewSourceStrategy
    {
        public SvgViewSourceStrategy(ArchiveEntry archiveEntry, PictureInfo? pictureInfo)
            : base(new SvgPictureSource(archiveEntry, pictureInfo))
        {
        }
    }

}
