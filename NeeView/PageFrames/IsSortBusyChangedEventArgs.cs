using System;

namespace NeeView.PageFrames
{
    public class IsSortBusyChangedEventArgs : EventArgs
    {
        public IsSortBusyChangedEventArgs(bool isSortBusy)
        {
            IsSortBusy = isSortBusy;
        }

        public bool IsSortBusy { get; }
    }
}
