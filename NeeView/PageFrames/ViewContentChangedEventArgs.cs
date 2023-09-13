using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        public FrameViewContentChangedEventArgs(ViewContentChangedAction action, IReadOnlyList<ViewContent> viewContents, int direction)
        {
            Debug.Assert(direction is -1 or +1);
            Action = action;
            ViewContents = viewContents;
            Direction = direction;
        }

        // TODO: PageFrameContent は大雑把すぎる？ ViewContent[] でそれぞれ ViewContentChangedAction を保持するように？
        //public PageFrameContent PageFrameContent { get; }

        public ViewContentChangedAction Action { get; }
        
        public IReadOnlyList<ViewContent> ViewContents { get; }

        public int Direction { get; }

        public ViewContentState State => ViewContents.Select(e => e.State).DefaultIfEmpty(ViewContentState.Loaded).Min();

        public ViewContentChangedEventArgs? InnerArgs { get; init; }


    }
}
