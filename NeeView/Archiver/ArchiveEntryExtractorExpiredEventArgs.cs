using System;

namespace NeeView
{
    public class ArchiveEntryExtractorExpiredEventArgs : EventArgs
    {
        public ArchiveEntryExtractorExpiredEventArgs(ArchiveEntry archiveEntry, AggregateException? exception)
        {
            ArchiveEntry = archiveEntry;
            Exception = exception;
        }

        public ArchiveEntry ArchiveEntry { get; }
        public AggregateException? Exception { get; }
    }
}
