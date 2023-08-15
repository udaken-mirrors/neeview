namespace NeeView
{
    public interface IMemoryElement
    {
        int Index { get; }
        bool IsMemoryLocked { get; }
        long GetMemorySize();
        void Unload();
    }

}
