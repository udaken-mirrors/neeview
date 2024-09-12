using System;

namespace NeeView
{
    public enum QuickAccessCollectionChangeAction
    {
        Add,
        Remove,
        Move,
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

        public QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction action, QuickAccess? element, int oldIndex, int newIndex)
        {
            if (action != QuickAccessCollectionChangeAction.Move) throw new ArgumentException("action is not Move");
            Action = action;
            Element = element;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }

        public QuickAccessCollectionChangeAction Action { get; }
        public QuickAccess? Element { get; }
        public int OldIndex { get; }
        public int NewIndex { get; }
    }
}
