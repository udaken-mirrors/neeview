namespace NeeView
{
    /// <summary>
    /// RenameControlの結果
    /// </summary>
    public class RenameControlResult
    {
        public RenameControlResult(string oldValue, string newValue, int moveRename, bool isRestoreFocus)
        {
            OldValue = oldValue;
            NewValue = newValue;
            MoveRename = moveRename;
            IsRestoreFocus = isRestoreFocus;
        }

        public string OldValue { get; }
        public string NewValue { get; }
        public int MoveRename { get; }
        public bool IsRestoreFocus { get; }

        public bool IsChanged => OldValue != NewValue;
    }
}
