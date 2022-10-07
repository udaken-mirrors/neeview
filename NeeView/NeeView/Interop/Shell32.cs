using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport("shell32")]
        internal static extern IntPtr SHAppBarMessage(AppBarMessages dwMessage, ref APPBARDATA pData);
    }
}
