using System;

namespace NeeView
{
    public record class BookItemAccessor
    {
        private readonly FolderItem _source;

        public BookItemAccessor(FolderItem source)
        {
            _source = source;
        }

        internal FolderItem Source => _source;

        [WordNodeMember]
        public string? Name => _source.DispName;

        [WordNodeMember]
        public string Path => _source.TargetPath.SimplePath;

        [WordNodeMember]
        public long Size => _source.Length;

        [WordNodeMember]
        public DateTime LastWriteTime => _source.LastWriteTime;

        [WordNodeMember]
        public DateTime CreationTime => _source.CreationTime;
    }

}
