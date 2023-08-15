#if false
using System;

namespace NeeView
{
    [Obsolete]
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
#endif