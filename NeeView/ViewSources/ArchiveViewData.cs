namespace NeeView
{
    public class ArchiveViewData
    {
        public ArchiveViewData(ArchiveEntry entry, ThumbnailBitmap thumbnail)
        {
            Entry = entry;
            Thumbnail = thumbnail;
        }

        public ArchiveEntry Entry { get; }
        public ThumbnailBitmap Thumbnail { get; }
    }
}