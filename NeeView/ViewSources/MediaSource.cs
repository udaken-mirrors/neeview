using System;
using System.IO;

namespace NeeView
{
    public class MediaSource : IHasCache
    {
        public MediaSource(IStreamSource? streamSource)
        {
            StreamSource = streamSource;
        }

        public MediaSource(string? path)
        {
            Path = path;
        }

        public IStreamSource? StreamSource { get; }

        public string? Path { get; }

        public long CacheSize => (StreamSource as IHasCache)?.CacheSize ?? 0;

        public bool IsValid() => StreamSource != null || Path != null;

        public override string ToString()
        {
            return Path ?? "StreamSource";
        }

        public Stream OpenStream()
        {
            if (StreamSource is not null)
            {
                return StreamSource.OpenStream();
            }
            else if (Path is not null)
            {
                return File.OpenRead(Path);
            }

            throw new InvalidOperationException();
        }

        public void ClearCache()
        {
            (StreamSource as IHasCache)?.ClearCache();
        }
    }
}
