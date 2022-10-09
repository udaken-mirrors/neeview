using System;

namespace NeeView.Interop
{
    [Flags]
    public enum SHImageLists
    {
        SHIL_LARGE = 0x0000, // 32x32
        SHIL_SMALL = 0x0001, // 16x16
        SHIL_EXTRALARGE = 0x0002, // 48x48 maybe.
        SHIL_SYSSMALL = 0x0003, // ?
        SHIL_JUMBO = 0x0004, //256x256 maybe.
    }
}
