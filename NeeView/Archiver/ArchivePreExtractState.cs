namespace NeeView
{
    public enum ArchivePreExtractState
    {
        None,
        Extracting,
        Canceled,
        Done,
        Failed,
        Sleep,
    }

    public static class ArchivePreExtractStateExtensions
    {
        public static bool IsReady(this ArchivePreExtractState state)
        {
            return state is ArchivePreExtractState.None or ArchivePreExtractState.Canceled;
        }

        public static bool IsCompleted(this ArchivePreExtractState state)
        {
            return state is ArchivePreExtractState.Canceled or ArchivePreExtractState.Done or ArchivePreExtractState.Failed or ArchivePreExtractState.Sleep;
        }
    }
}

