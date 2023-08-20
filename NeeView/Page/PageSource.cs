using NeeView.ComponentModel;

namespace NeeView
{
    public class PageSource : IDataSource
    {
        public PageSource(object? data, string? errorMessage, PictureInfo? pictureInfo)
        {
            Data = data;
            ErrorMessage = errorMessage;
            PictureInfo = pictureInfo;
        }

        public object? Data { get; }
        public virtual long DataSize => 0;
        public string? ErrorMessage { get; }
        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        public DataState DataState => IsFailed ? DataState.Failed : IsLoaded ? DataState.Loaded : DataState.None;

        public PictureInfo? PictureInfo { get; }


        public static PageSource CreateEmpty()
        {
            return new PageSource(null, null, null);
        }

        public static PageSource Create(object data, PictureInfo? pictureInfo)
        {
            return new PageSource(data, null, pictureInfo);
        }

        public static PageSource CreateError(string errorMessage)
        {
            return new PageSource(null, errorMessage, null);
        }
    }

}
