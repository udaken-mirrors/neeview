namespace NeeView.Interop
{
    public enum AppBarEdges : uint
    {
        /// <summary>
        /// Left edge.
        /// </summary>
        ABE_LEFT = 0,

        /// <summary>
        /// Top edge.
        /// </summary>
        ABE_TOP = 1,

        /// <summary>
        /// Right edge.
        /// </summary>
        ABE_RIGHT = 2,

        /// <summary>
        /// Bottom edge.
        /// </summary>
        ABE_BOTTOM = 3,

        /// <summary>
        /// AppBarが存在しない場合の値。Win32API非対応。
        /// </summary>
        None = 0xFFFF,
    }

}
