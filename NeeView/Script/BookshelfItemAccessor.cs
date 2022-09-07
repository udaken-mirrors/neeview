namespace NeeView
{
    public class BookshelfItemAccessor
    {
        private readonly FolderItem _source;

        public BookshelfItemAccessor(FolderItem source)
        {
            _source = source;
        }

        internal FolderItem Source => _source;

        [WordNodeMember]
        public string? Name => _source.DispName;

        [WordNodeMember]
        public string Path => _source.TargetPath.SimplePath;
    }
}
