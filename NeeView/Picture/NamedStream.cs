using System;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// 名前とセットのストリーム
    /// </summary>
    public sealed class NamedStream : IDisposable
    {
        public Stream Stream { get; set; }
        public string? Name { get; set; }
        public byte[]? RawData { get; set; }

        public NamedStream(Stream stream, string? name, byte[]? rawData)
        {
            this.Stream = stream;
            this.Name = name;
            this.RawData = rawData;
        }

        public void Dispose()
        {
            this.Stream?.Dispose();
        }
    }

}
