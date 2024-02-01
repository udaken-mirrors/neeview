using System.IO;

namespace NeeView
{
    /// <summary>
    /// メモリバッファを StreamSource にする
    /// </summary>
    public class MemoryStreamSource : IStreamSource
    {
        public MemoryStreamSource(byte[] bytes)
        {
            Bytes = bytes;
        }

        public long Length => Bytes.LongLength;

        public byte[] Bytes { get; }

        public Stream OpenStream()
        {
            return new MemoryStream(Bytes);
        }

        public long GetMemorySize() => Length;
    }

}
