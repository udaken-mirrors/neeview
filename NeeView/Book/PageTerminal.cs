using System;

namespace NeeView
{
    [Flags]
    public enum PageTerminal
    {
        None = 0,
        First = 1 << 0,
        Last = 1 << 1
    }
}
