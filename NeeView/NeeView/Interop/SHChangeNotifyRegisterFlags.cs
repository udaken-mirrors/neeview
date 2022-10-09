using System;

namespace NeeView.Interop
{
    [Flags]
    public enum SHChangeNotifyRegisterFlags
    {
        SHCNRF_InterruptLevel = 0x1,
        SHCNRF_ShellLevel = 0x2,
        SHCNRF_RecursiveInterrupt = 0x1000,
        SHCNRF_NewDelivery = 0x8000,
    }
}
