namespace NeeView
{
    /// <summary>
    /// 画像、もしくはサムネイルタイプを指定するもの
    /// </summary>
    public class ThumbnailSource
    {

        public ThumbnailSource(ThumbnailType type) : this(type, null)
        {
        }

        public ThumbnailSource(byte[]? rawData) : this(ThumbnailType.Unique, rawData)
        {
        }

        public ThumbnailSource(ThumbnailType type, byte[]? rawData)
        {
            Type = type;
            RawData = rawData;
        }

        public ThumbnailType Type { get; set; }
        public byte[]? RawData { get; set; }

    }

}
