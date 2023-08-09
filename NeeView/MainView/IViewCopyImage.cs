namespace NeeView
{
    public interface IViewCopyImage
    {
        bool CanCopyImageToClipboard();
        void CopyImageToClipboard();
    }
}