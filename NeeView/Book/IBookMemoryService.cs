namespace NeeView
{
    public interface IBookMemoryService
    {
        bool IsFull { get; }
        long TotalSize { get; }

        void AddPageContent(IMemoryElement content);
        void AddPictureSource(IMemoryElement pictureSource);
        void CleanupDeep();
    }
}
