using System;
using System.Diagnostics;
using System.IO;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntry を StreamSourceにする
    /// </summary>
    public class ArchiveEntryStreamSource : IStreamSource, IHasCache
    {
        private byte[]? _cache;

        public ArchiveEntryStreamSource(ArchiveEntry archiveEntry)
        {
            ArchiveEntry = archiveEntry;
        }

        public ArchiveEntry ArchiveEntry { get; }
        
        public long Length => ArchiveEntry.Length;

        public long CacheSize => _cache?.Length ?? 0;

        public Stream OpenStream()
        {
            // 展開処理の重複を避けるため、ファイルシステムエントリ以外はキャッシュを作る
            if (_cache is null && !ArchiveEntry.HasCache && !ArchiveEntry.IsFileSystem)
            {
                using var stream = ArchiveEntry.OpenEntry();
                _cache = stream.ToArray(0, (int)ArchiveEntry.Length);
                Debug.Assert(ArchiveEntry.Length == _cache.Length);
            }

            if (_cache is not null)
            {
                return new MemoryStream(_cache, false);
            }
            else
            {
                return ArchiveEntry.OpenEntry();
            }
        }

        public ReadOnlySpan<byte> GetSpan()
        {
            if (_cache is not null)
            {
                return new ReadOnlySpan<byte>(_cache);
            }
            else
            {
                using var stream = ArchiveEntry.OpenEntry();
                return stream.ToSpan(0, (int)ArchiveEntry.Length);
            }
        }

        public void ClearCache()
        {
            _cache = null;
        }
    }


    public interface IHasCache
    {
        long CacheSize { get; }

        void ClearCache();
    }

}