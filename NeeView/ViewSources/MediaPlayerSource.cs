using System;

namespace NeeView
{
    /// <summary>
    /// NOTE: まだ未使用
    /// </summary>
    [Obsolete]
    public class MediaPlayerSource
    {
        public MediaPlayerSource(string path)
        {
            Path = path;
        }

        public string Path { get; }
        public TimeSpan Position { get; set; }
    }
}
