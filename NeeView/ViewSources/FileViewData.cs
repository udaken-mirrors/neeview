namespace NeeView
{
    public class FileViewData
    {
        public FileViewData(FilePageData pageData)
            : this(pageData.Entry, pageData.Icon, pageData.Message)
        {
        }

        public FileViewData(ArchiveEntry entry, FilePageIcon icon, string? message)
        {
            Entry = entry;
            Icon = icon;
            Message = message;
        }

        public ArchiveEntry Entry { get; }
        public FilePageIcon Icon { get; }
        public string? Message { get; }
    }

}
