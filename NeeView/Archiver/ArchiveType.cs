
namespace NeeView
{
    /// <summary>
    /// アーカイバーの種類
    /// </summary>
    public enum ArchiveType
    {
        None,

        FolderArchive,
        ZipArchive,
        SevenZipArchive,
        PdfArchive,
        SusieArchive,
        MediaArchive,
        PlaylistArchive,
    }

    public static class ArchiveTypeExtensions
    {
        // 多重圧縮ファイルが可能なアーカイブであるか
        public static bool IsRecursiveSupported(this ArchiveType self)
        {
            return self switch
            {
                ArchiveType.PdfArchive or ArchiveType.MediaArchive => false,
                _ => true,
            };
        }
    }
}

