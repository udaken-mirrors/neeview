namespace NeeView
{
    public class ArchiveViewData
    {
        public ArchiveViewData(ArchiveEntry entry, Thumbnail thumbnail)
        {
            Entry = entry;
            Thumbnail = thumbnail;
        }

        public ArchiveEntry Entry { get; }
        public Thumbnail Thumbnail { get; }
    }
}