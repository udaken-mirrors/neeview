using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHChangeNotifyEntry
    {
        public IntPtr pIdl;
        [MarshalAs(UnmanagedType.Bool)] public Boolean Recursively;
    }
}
