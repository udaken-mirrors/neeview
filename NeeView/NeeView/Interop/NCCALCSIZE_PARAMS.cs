using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NCCALCSIZE_PARAMS
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public RECT[] rgrc;
        public WINDOWPOS lppos;
    }

}
