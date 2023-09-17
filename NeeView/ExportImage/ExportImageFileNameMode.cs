namespace NeeView
{
    public enum ExportImageFileNameMode
    {
        [AliasName]
        Original,

        [AliasName]
        BookPageNumber,

        [AliasName(IsVisible = false)]
        Default,
    }
}
