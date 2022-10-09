using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;


namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct FILEDESCRIPTORW
    {
        public int dwFlags;
        public Guid clsid;
        public long sizel;
        public long pointl;
        public int dwFileAttributes;
        public FILETIME ftCreationTime;
        public FILETIME ftLastAccessTime;
        public FILETIME ftLastWriteTime;
        public int nFileSizeHigh;
        public uint nFileSizeLow;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string cFileName;
    }
}
