namespace NeeView
{
    /// <summary>
    /// ArchiveEntryCollectionの展開範囲
    /// </summary>
    public enum ArchiveEntryCollectionMode
    {
        /// <summary>
        /// 指定ディレクトリのみ
        /// </summary>
        [AliasName]
        CurrentDirectory,

        /// <summary>
        /// 指定ディレクトリを含む現在のアーカイブの範囲
        /// </summary>
        [AliasName]
        IncludeSubDirectories,

        /// <summary>
        /// 指定ディレクトリ以下サブアーカイブ含むすべて
        /// </summary>
        [AliasName]
        IncludeSubArchives,
    }
}
