namespace NeeView
{
    public interface IBookSetting : IBookPageViewSetting
    {
        public string Page { get; set; }
        public bool IsRecursiveFolder { get; set; }
        public PageSortMode SortMode { get; set; }

        bool IsEquals(IBookSetting? other)
        {
            return other is not null &&
                ((IBookPageViewSetting)this).IsEquals(other) &&
                Page == other.Page &&
                IsRecursiveFolder == other.IsRecursiveFolder &&
                SortMode == other.SortMode;
        }

        bool IsSettingEquals(IBookSetting? other)
        {
            return other is not null &&
                ((IBookPageViewSetting)this).IsEquals(other) &&
                IsRecursiveFolder == other.IsRecursiveFolder &&
                SortMode == other.SortMode;
        }

    }
}
