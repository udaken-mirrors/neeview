using System;

namespace NeeView
{
    public interface IOpenableMediaPlayer : IMediaPlayer
    {
        void Open(MediaSource mediaSource, TimeSpan delay);
    }
}
