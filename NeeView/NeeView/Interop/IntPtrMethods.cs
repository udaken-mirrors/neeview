using System;

namespace NeeView.Interop
{
    internal static class IntPtrMethods
    {
        internal static ushort HIWORD(IntPtr dwValue)
        {
            return (ushort)((((long)dwValue) >> 0x10) & 0xffff);
        }

        internal static ushort HIWORD(uint dwValue)
        {
            return (ushort)(dwValue >> 0x10);
        }

        internal static int GET_WHEEL_DELTA_WPARAM(IntPtr wParam)
        {
            return (short)HIWORD(wParam);
        }

        internal static int GET_WHEEL_DELTA_WPARAM(uint wParam)
        {
            return (short)HIWORD(wParam);
        }
    }
}
