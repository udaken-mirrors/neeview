using System;
using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    //[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    //If you use the above you may encounter an invalid memory access exception (when using ANSI
    //or see nothing (when using unicode) when you use FOF_SIMPLEPROGRESS flag.
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SHFILEOPSTRUCT
    {
        public IntPtr hwnd;
        public FileFuncFlags wFunc;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pFrom;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? pTo;
        public FileOperationFlags fFlags;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string? lpszProgressTitle;
    }
}
