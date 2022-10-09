using System;

namespace NeeView.Interop
{
    [Flags]
    public enum FileOperationFlags : ushort
    {
        FOF_MULTIDESTFILES = 0x1,
        FOF_CONFIRMMOUSE = 0x2,
        FOF_SILENT = 0x4,
        FOF_RENAMEONCOLLISION = 0x8,
        FOF_NOCONFIRMATION = 0x10,
        FOF_WANTMAPPINGHANDLE = 0x20,
        FOF_ALLOWUNDO = 0x40,
        FOF_FILESONLY = 0x80,
        FOF_SIMPLEPROGRESS = 0x100,
        FOF_NOCONFIRMMKDIR = 0x200,
        FOF_NOERRORUI = 0x400,
        FOF_NOCOPYSECURITYATTRIBS = 0x800,
        FOF_NORECURSION = 0x1000,
        FOF_NO_CONNECTED_ELEMENTS = 0x2000,
        FOF_WANTNUKEWARNING = 0x4000,
        FOF_NORECURSEREPARSE = 0x8000
    }
}
