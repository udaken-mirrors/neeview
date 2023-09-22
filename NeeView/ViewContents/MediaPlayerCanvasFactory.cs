using NeeView.PageFrames;
using System;
using System.Windows;

namespace NeeView
{
    public static class MediaPlayerCanvasFactory
    {
        public static MediaPlayerCanvas Create(PageFrameElement element, MediaViewData source, ViewContentSize contentSize, Rect viewbox, IMediaPlayer player)
        {
            switch (player)
            {
                case DefaultCoreMediaPlayer mediaPlayer:
                    return new DefaultMediaPlayerCanvas(element, source, contentSize, viewbox, mediaPlayer);
                case VlcCoreMediaPlayer vlcMediaPlayer:
                    return new VlcMediaPlayerCanvas(element, source, contentSize, viewbox, vlcMediaPlayer);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
