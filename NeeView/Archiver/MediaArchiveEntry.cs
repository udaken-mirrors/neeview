namespace NeeView
{
    public class MediaArchiveEntry : ArchiveEntry
    {
        public MediaArchiveEntry(Archiver archiver) : base(archiver)
        {
        }

        public override string EntryFullName => Archiver.SystemPath;

        public override string SystemPath => Archiver.SystemPath;

        public override string? EntityPath => Archiver.Path;

        public override string Ident => Archiver.Ident;
    }
}
