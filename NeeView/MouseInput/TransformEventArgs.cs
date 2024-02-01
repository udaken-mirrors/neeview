using System;

namespace NeeView
{
    // 変化通知イベントの引数
    public class TransformEventArgs : EventArgs
    {
        public TransformEventArgs(TransformActionType actionType)
        {
            this.ActionType = actionType;
        }

        /// <summary>
        /// 変化したもの
        /// </summary>
        public TransformActionType ActionType { get; set; }
    }


}
