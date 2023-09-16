namespace NeeView
{
    public class PdfPageData : IHasRawData
    {
        public PdfPageData(ArchiveEntry archiveEntry)
        {
            ArchiveEntry = archiveEntry;
        }

        public ArchiveEntry ArchiveEntry { get; }

        public object? RawData => ArchiveEntry;
    }
}