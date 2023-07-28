using System;

namespace NeeView
{
    public class SelectedRangeChangedEventArgs : EventArgs
    {
        public SelectedRangeChangedEventArgs(bool fromOutsize)
        {
            // 外界から
            FromOutsize = fromOutsize;
        }

        public bool FromOutsize { get; }
    }
}
