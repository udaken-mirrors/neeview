namespace NeeView
{
    public record class BookshelfItemAccessor : BookItemAccessor
    {
        public BookshelfItemAccessor(FolderItem source) : base(source)
        {
        }
    }
}
