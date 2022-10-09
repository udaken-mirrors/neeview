using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("comctl32.dll", SetLastError = true)]
        internal static extern IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);
    }
}
