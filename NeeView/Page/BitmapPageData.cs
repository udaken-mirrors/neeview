namespace NeeView
{
    public class BitmapPageData : IHasStreamSource, IHasRawData
    {
        public BitmapPageData(IStreamSource streamSource)
        {
            StreamSource = streamSource;
        }

        public IStreamSource StreamSource { get; }

        public object? RawData => StreamSource;
    }

}