using System;

namespace NeeView
{
    public enum QuickAccessCollectionChangeAction
    {
        Add,
        Remove,
        Refresh,
        Rename,
        PathChanged,
    }


    public class QuickAccessCollectionChangeEventArgs : EventArgs
    {
        public QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction action, QuickAccess? element)
        {
            Action = action;
            Element = element;
        }

        public QuickAccessCollectionChangeAction Action { get; }
        public QuickAccess? Element { get; }
    }
}
