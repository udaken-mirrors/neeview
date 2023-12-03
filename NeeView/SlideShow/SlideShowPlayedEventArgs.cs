using System;

namespace NeeView
{
    public class SlideShowPlayedEventArgs : EventArgs
    {
        public SlideShowPlayedEventArgs(bool isPlaying, double interval)
        {
            IsPlaying = isPlaying;
            IntervalMilliseconds = interval;
        }

        public bool IsPlaying { get; }
        public double IntervalMilliseconds { get; }
    }

}
