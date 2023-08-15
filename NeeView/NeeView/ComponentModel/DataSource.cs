namespace NeeView.ComponentModel
{
    public record class DataSource : IDataSource
    {
        public DataSource()
        {
        }

        public DataSource(object? data, long dataSize, string? errorMessage)
        {
            Data = data;
            DataSize = dataSize;
            ErrorMessage = errorMessage;
        }

        public object? Data { get; }
        public long DataSize { get; }
        public string? ErrorMessage { get; }

        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        public DataState DataState => IsFailed ? DataState.Failed : IsLoaded ? DataState.Loaded : DataState.None;
    }





}