namespace NeeView
{
    public class BitmapPageData : IHasByteArray, IHasRawData
    {
        public BitmapPageData(byte[] bytes)
        {
            Bytes = bytes;
        }

        public byte[] Bytes { get; }

        public object? RawData => Bytes;
    }
}