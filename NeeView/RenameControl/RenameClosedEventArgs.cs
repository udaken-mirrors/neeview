using System;

namespace NeeView
{
    public class RenameClosedEventArgs : EventArgs
    {
        public RenameClosedEventArgs(string oldValue, string newValue, int moveRename, bool isRestoreFocus)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MoveRename = moveRename;
            IsRestoreFocus = isRestoreFocus;
        }

        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int MoveRename { get; set; }
        public bool IsRestoreFocus { get; set; }

        public bool IsChanged => OldValue != NewValue;
    }
}
