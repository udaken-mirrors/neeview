using System.Runtime.InteropServices;


namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct FILEGROUPDESCRIPTORW
    {
        public int cItems;
        // NOTE: この属性を削除するとメモリエラーでアプリが終了する
        // NTOE: CS9125 警告が発生する。SizeConst指定が必要だが、可変長の場合はどうすれば？
        [MarshalAs(UnmanagedType.ByValArray)] 
        public FILEDESCRIPTORW[] fgd;
    }
}
