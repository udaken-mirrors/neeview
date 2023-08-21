namespace NeeView
{
    /// <summary>
    /// 画像、もしくはサムネイルタイプを指定するもの
    /// </summary>
    public class ThumbnailSource
    {
        public ThumbnailType Type { get; set; }
        public byte[]? RawData { get; set; }

        public ThumbnailSource(ThumbnailType type)
        {
            Type = type;
        }

        public ThumbnailSource(byte[]? rawData)
        {
            Type = ThumbnailType.Unique;
            RawData = rawData;
        }
    }

}
