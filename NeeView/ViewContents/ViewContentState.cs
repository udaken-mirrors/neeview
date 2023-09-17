using NeeView.PageFrames;
using System.ComponentModel;

namespace NeeView
{
    public enum ViewContentState
    {
        None,
        Loading,
        Loaded,
        Failed,
    }

    public static class ViewContentStateExtensions
    {
        public static ViewContentChangedAction ToChangedAction(this ViewContentState state)
        {
            return state switch
            {
                ViewContentState.None => ViewContentChangedAction.ContentLoading,
                ViewContentState.Loading => ViewContentChangedAction.ContentLoading,
                ViewContentState.Loaded => ViewContentChangedAction.ContentLoaded,
                ViewContentState.Failed => ViewContentChangedAction.ContentFailed,
                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}
