using System;

namespace NeeView
{
    public class CollectionChangedEventArgs<T> : EventArgs
        where T : class
    {
        public CollectionChangedEventArgs(CollectionChangedAction action, T? item) : this(action, item, null)
        {
        }

        public CollectionChangedEventArgs(CollectionChangedAction action, T? item, T? oldItem)
        {
            Action = action;
            Item = item;
            OldItem = oldItem;
        }

        public CollectionChangedAction Action { get; }
        public T? Item { get; }
        public T? OldItem { get; init; }
    }

    public enum CollectionChangedAction
    {
        Add,
        Remove,
        Rename,
    }

}
