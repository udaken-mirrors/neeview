using System;

namespace NeeView
{
    /// <summary>
    /// ロード用
    /// </summary>
    public class ThumbnailCacheRecord
    {
        public ThumbnailCacheRecord(byte[] bytes, DateTime dateTime)
        {
            Bytes = bytes;
            DateTime = dateTime;
        }

        public byte[] Bytes { get; private set; }
        public DateTime DateTime { get; private set; }
    }
}
