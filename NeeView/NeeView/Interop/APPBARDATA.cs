using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public AppBarEdges uEdge;
        public RECT rc;
        public IntPtr lParam;

        public static APPBARDATA Create()
        {
            var appBarData = new APPBARDATA();
            appBarData.cbSize = Marshal.SizeOf(typeof(APPBARDATA));
            return appBarData;
        }
    }

}
