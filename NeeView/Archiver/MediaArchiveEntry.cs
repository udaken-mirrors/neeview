namespace NeeView
{
    public class MediaArchiveEntry : ArchiveEntry
    {
        public MediaArchiveEntry(Archive archiver) : base(archiver)
        {
        }

        public override string EntryFullName => Archive.SystemPath;

        public override string SystemPath => Archive.SystemPath;

        public override string? EntityPath => Archive.Path;

        public override string Ident => Archive.Ident;
    }
}
