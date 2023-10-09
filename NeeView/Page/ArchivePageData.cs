namespace NeeView
{
    public class ArchivePageData
    {
        public ArchivePageData(ArchiveEntry archiveEntry, Thumbnail thumbnail)
        {
            ArchiveEntry = archiveEntry;
            Thumbnail = thumbnail;
        }

        public ArchiveEntry ArchiveEntry { get; }
        public Thumbnail Thumbnail { get; }
    }

}
