using System;

namespace NeeView
{
    public interface ICoreMediaPlayer : IMediaPlayer
    {
        void Open(Uri uri, TimeSpan delay);
    }
}