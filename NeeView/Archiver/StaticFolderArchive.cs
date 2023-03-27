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
        public ArchiveEntry CreateArchiveEntry(string path, bool isForce = false)
        {
            var fullpath = System.IO.Path.GetFullPath(path);
            var directoryInfo = new DirectoryInfo(fullpath);
            if (directoryInfo.Exists)
            {
                return CreateArchiveEntry(directoryInfo, 0);
            }
            var fileInfo = new FileInfo(fullpath);
            if (fileInfo.Exists)
            {
                return CreateArchiveEntry(fileInfo, 0);
            }
            else if (isForce)
            {
                return new ArchiveEntry(this) { RawEntryName = path };
            }

            throw new FileNotFoundException("File not found.", fullpath);
        }
    }
}
