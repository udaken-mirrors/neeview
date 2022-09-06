namespace NeeView
{
    /// <summary>
    /// 保存キュー用
    /// </summary>
    public class ThumbnailCacheItem
    {
        public ThumbnailCacheItem(ThumbnailCacheHeader header, byte[] body)
        {
            Header = header;
            Body = body;
        }

        public ThumbnailCacheHeader Header { get; set; }
        public byte[] Body { get; set; }
    }
}
