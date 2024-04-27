using System;

namespace NeeView
{
    /// <summary>
    /// ジェスチャーイベントデータ
    /// </summary>
    public class MouseGestureEventArgs : EventArgs
    {
        /// <summary>
        /// ジェスチャー
        /// </summary>
        public MouseSequence Sequence { get; set; }

        /// <summary>
        /// 処理済フラグ
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="sequence"></param>
        public MouseGestureEventArgs(MouseSequence sequence)
        {
            Sequence = sequence;
        }
    }
}
