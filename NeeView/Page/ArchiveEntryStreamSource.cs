using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntry を StreamSourceにする
    /// </summary>
    public class ArchiveEntryStreamSource : IStreamSource, IHasCache
    {
        private MemoryStream? _memoryStream;

        public ArchiveEntryStreamSource(ArchiveEntry archiveEntry)
        {
            ArchiveEntry = archiveEntry;

        }

        public ArchiveEntry ArchiveEntry { get; }
        
        public long Length => ArchiveEntry.Length;

        public long CacheSize => _memoryStream?.Capacity ?? 0;

        public Stream OpenStream()
        {
            // 展開処理の重複を避けるため、ファイルシステムエントリ以外はメモリキャッシュを作る
            if (_memoryStream is null && !ArchiveEntry.HasCache && !ArchiveEntry.IsFileSystem)
            {
                _memoryStream = new MemoryStream();
                using var stream = ArchiveEntry.OpenEntry();
                stream.CopyTo(_memoryStream);
                _memoryStream.Seek(0, SeekOrigin.Begin);
                Debug.Assert(ArchiveEntry.Length == _memoryStream.Length);

                var a = _memoryStream.ToArray();
                var b = stream.ToArray();

            }

            if (_memoryStream is not null)
            {
                return new MemoryStream(_memoryStream.GetBuffer(), 0, (int)_memoryStream.Length, false);
            }
            else
            {
                return ArchiveEntry.OpenEntry();
            }
        }

        public void ClearCache()
        {
            _memoryStream = null;
        }
    }


    public interface IHasCache
    {
        long CacheSize { get; }

        void ClearCache();
    }

}