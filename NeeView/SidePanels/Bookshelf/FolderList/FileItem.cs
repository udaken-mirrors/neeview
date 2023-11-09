#define LOCAL_DEBUG
using NeeLaboratory.IO.Search;
//using NeeLaboratory.IO.Search.FileNode;
//using NeeLaboratory.IO.Search.FileSearch;
using System;
using System.IO;

namespace NeeView
{
    public class FileItem : ISearchItem
    {
        private FileSystemInfo _info;

        public FileItem(FileSystemInfo fileSystemInfo)
        {
            _info = fileSystemInfo;
        }

        public FileSystemInfo FileSystemInfo => _info;

        public bool IsDirectory => _info.Attributes.HasFlag(FileAttributes.Directory);

        public string Path => _info.FullName;

        public DateTime LastWriteTime => _info.LastWriteTime;

        public long Size => _info is System.IO.FileInfo fileInfo ? fileInfo.Length : -1;

        public SearchValue GetValue(SearchPropertyProfile profile)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(_info.Name);
                case "date":
                    return new DateTimeSearchValue(_info.LastWriteTime);
                case "isdir":
                    return new BooleanSearchValue(IsDirectory);
                default:
                    throw new NotSupportedException();
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }

}
