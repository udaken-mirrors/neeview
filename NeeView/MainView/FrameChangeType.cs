//#define LOCAL_DEBUG

namespace NeeView
{
    public enum FrameChangeType
    {
        None,

        /// <summary>
        /// フレームサイズの変更
        /// </summary>
        Size,

        /// <summary>
        /// フレームレイアウトの変更
        /// </summary>
        Layout,

        /// <summary>
        /// フレーム範囲の両端の変更
        /// </summary>
        Range,

        /// <summary>
        /// 範囲の片端の変更  
        /// </summary>
        RangeSize,
    }
}
