using System.IO;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntry を StreamSourceにする
    /// </summary>
    public class ArchiveEntryStreamSource : IStreamSource
    {
        public ArchiveEntryStreamSource(ArchiveEntry archiveEntry)
        {
            ArchiveEntry = archiveEntry;
        }

        public long Length => ArchiveEntry.Length;

        public ArchiveEntry ArchiveEntry { get; }

        public Stream OpenStream()
        {
            return ArchiveEntry.OpenEntry();
        }
    }

}