using System.Runtime.InteropServices;


namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct FILEGROUPDESCRIPTORW
    {
        public int cItems;
        [MarshalAs(UnmanagedType.ByValArray)]
        public FILEDESCRIPTORW[] fgd;
    }
}
