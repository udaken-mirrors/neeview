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
        ContentLoading,
        ContentLoaded,
        ContentFailed,

        /// <summary>
        /// 選択コンテンツの変更
        /// </summary>
        Selection,
    }


    public static class ViewContentChangedActionExtensions
    {
        public static ViewContentChangedAction Min(ViewContentChangedAction a, ViewContentChangedAction b)
        {
            return (a < b) ? a : b;
        }
    }


    public class ViewContentChangedEventArgs : EventArgs
    {
        public ViewContentChangedEventArgs(ViewContentChangedAction action, ViewContent viewContent)
        {
            Action = action;
            ViewContent = viewContent;
        }

        public ViewContent ViewContent { get; }

        public ViewContentChangedAction Action { get; }
    }


    public class FrameViewContentChangedEventArgs : EventArgs
    {
        public FrameViewContentChangedEventArgs(ViewContentChangedAction action, PageFrameContent pageFrameContent)
        {
            Action = action;
            PageFrameContent = pageFrameContent;
        }

        // TODO: PageFrameContent は大雑把すぎる？ ViewContent[] でそれぞれ ViewContentChangedAction を保持するように？
        public PageFrameContent PageFrameContent { get; }

        public ViewContentChangedAction Action { get; }

        public ViewContentChangedEventArgs? InnerArgs { get; init; }
    }
}
