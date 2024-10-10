namespace NeeView
{
    /// <summary>
    /// ArchiveEntry に親子関係を添付したもの。
    /// PlaylistArchive を展開したときに ArchiveEntry だけでは正しい親子関係を取得できないため。
    /// </summary>
    public class ArchiveEntryNode
    {
        public ArchiveEntryNode(ArchiveEntryNode? parent, ArchiveEntry archiveEntry)
        {
            Parent = parent;
            ArchiveEntry = archiveEntry;
        }

        public ArchiveEntryNode? Parent { get; init; }
        public ArchiveEntry ArchiveEntry { get; init; }

        public string Path => LoosePath.Combine(Parent?.Path, ArchiveEntry.EntryName);
    }
}
