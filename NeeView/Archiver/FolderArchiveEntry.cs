namespace NeeView
{
    public class FolderArchiveEntry : ArchiveEntry
    {
        public FolderArchiveEntry(Archiver archiver) : base(archiver)
        {
        }

        public string? Link { get; set; }

        public override string PlacePath => SystemPath;

        public override string? EntityPath => Link ?? SystemPath;

        public override bool IsFileSystem => true;

        public override bool IsShortcut => Link is not null || base.IsShortcut;
    }
}
