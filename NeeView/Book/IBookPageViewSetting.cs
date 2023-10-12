namespace NeeView
{
    public interface IBookPageViewSetting
    {
        PageReadOrder BookReadOrder { get; set; }
        bool IsSupportedDividePage { get; set; }
        bool IsSupportedSingleFirstPage { get; set; }
        bool IsSupportedSingleLastPage { get; set; }
        bool IsSupportedWidePage { get; set; }
        PageMode PageMode { get; set; }
        AutoRotateType AutoRotate { get; set; }
    }
}