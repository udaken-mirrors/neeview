using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    /// <summary>
    /// ファイル単体用アーカイブ
    /// </summary>
    public class StaticFolderArchive : FolderArchive
    {
        public static StaticFolderArchive Default { get; } = new StaticFolderArchive();


        public StaticFolderArchive() : base("", null)
        {
        }


        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            return await Task.FromResult(new List<ArchiveEntry>());
        }

        /// <summary>
        /// エントリ作成
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="isForce">パスが存在しなくてもエントリを作成する</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">パスが存在しない</exception>
        public FolderArchiveEntry CreateArchiveEntry(string path, bool isForce = false)
        {
            var fullPath = System.IO.Path.GetFullPath(path);
            var directoryInfo = new DirectoryInfo(fullPath);
            if (directoryInfo.Exists)
            {
                return CreateArchiveEntry(directoryInfo, 0);
            }
            var fileInfo = new FileInfo(fullPath);
            if (fileInfo.Exists)
            {
                return CreateArchiveEntry(fileInfo, 0);
            }
            else if (isForce)
            {
                return new FolderArchiveEntry(this) { RawEntryName = path, IsTemporary = true };
            }

            throw new FileNotFoundException("File not found.", fullPath);
        }
    }
}
