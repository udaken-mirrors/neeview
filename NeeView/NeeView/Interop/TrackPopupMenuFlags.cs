using System;

namespace NeeView.Interop
{
    [Flags]
    public enum TrackPopupMenuFlags : uint
    {
        TPM_LEFTALIGN = 0x0000,
        TPM_CENTERALIGN = 0x0004,
        TPM_RIGHTALIGN = 0x0008,

        TPM_TOPALIGN = 0x0000,
        TPM_VCENTERALIGN = 0x0010,
        TPM_BOTTOMALIGN = 0x0020,

        TPM_NONOTIFY = 0x0080,
        TPM_RETURNCMD = 0x0100,
        TPM_LEFTBUTTON = 0x0000,
        TPM_RIGHTBUTTON = 0x0002,

        TPM_HORIZONTAL = 0x0000,
        TPM_VERTICAL = 0x0040,
    }
}
