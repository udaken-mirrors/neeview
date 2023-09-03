using System;


namespace NeeView.PageFrames
{
    /// <summary>
    /// ViewContentChanged イベント原因
    /// </summary>
    public enum ViewContentChangedAction
    {
        /// <summary>
        /// 変化なし
        /// </summary>
        None,

        /// <summary>
        /// コンテンツサイズの変更
        /// </summary>
        Size,

        /// <summary>
        /// コンテンツ自体の変更
        /// </summary>
        Content,

        /// <summary>
        /// 選択コンテンツの変更
        /// </summary>
        Selection,
    }



    public class ViewContentChangedEventArgs : EventArgs
    {
        public ViewContentChangedEventArgs(ViewContentChangedAction action)
        {
            Action = action;
        }

        public ViewContentChangedAction Action { get; }
    }


    public class FrameViewContentChangedEventArgs : ViewContentChangedEventArgs
    {
        public FrameViewContentChangedEventArgs(ViewContentChangedAction action, PageFrameContent pageFrameContent)
            : base(action)
        {
            PageFrameContent = pageFrameContent;
        }

        // TODO: PageFrameContent は大雑把すぎる？ ViewContent[] でそれぞれ ViewContentChangedAction を保持するように？
        public PageFrameContent PageFrameContent { get; }
    }
}
