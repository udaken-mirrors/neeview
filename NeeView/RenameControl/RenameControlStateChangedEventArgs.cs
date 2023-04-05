using System;

namespace NeeView
{
    public class RenameControlStateChangedEventArgs : EventArgs
    {
        public RenameControlStateChangedEventArgs(RenameControlStateChangedAction action)
        {
            Action = action;
        }

        public RenameControlStateChangedAction Action { get; }
    }

    public enum RenameControlStateChangedAction
    {
        Unloaded,
        SelectionChanged,
        ScrollChanged,
        LayoutChanged,
    };
}
