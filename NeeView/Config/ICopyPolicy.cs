namespace NeeView
{
    public interface ICopyPolicy
    {
        ArchivePolicy ArchiveCopyPolicy { get; set; }
        TextCopyPolicy TextCopyPolicy { get; set; }
    }
}
