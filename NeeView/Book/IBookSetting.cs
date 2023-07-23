namespace NeeView
{
    public interface IBookSetting : IBookPageViewSetting
    {
        public string Page { get; set; }
        public bool IsRecursiveFolder { get; set; }
        public PageSortMode SortMode { get; set; }
    }
}