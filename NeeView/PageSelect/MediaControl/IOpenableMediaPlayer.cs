using System;

namespace NeeView
{
    public interface IOpenableMediaPlayer : IMediaPlayer
    {
        void Open(Uri uri, TimeSpan delay);
    }
}