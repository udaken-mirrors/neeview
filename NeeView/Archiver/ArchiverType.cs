
namespace NeeView
{
    /// <summary>
    /// アーカイバーの種類
    /// </summary>
    public enum ArchiverType
    {
        None,

        FolderArchive,
        ZipArchiver,
        SevenZipArchiver,
        PdfArchiver,
        SusieArchiver,
        MediaArchiver,
        PlaylistArchiver,
    }

    public static class ArchiverTypeExtensions
    {
        // 多重圧縮ファイルが可能なアーカイブであるか
        public static bool IsRecursiveSupported(this ArchiverType self)
        {
            return self switch
            {
                ArchiverType.PdfArchiver or ArchiverType.MediaArchiver => false,
                _ => true,
            };
        }
    }
}

