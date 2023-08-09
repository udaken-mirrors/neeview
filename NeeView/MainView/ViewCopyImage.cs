namespace NeeView
{
    public class ViewCopyImage : IViewCopyImage
    {
        private readonly MainViewComponent _viewComponent;

        public ViewCopyImage(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
        }

        public bool CanCopyImageToClipboard()
        {
            return _viewComponent.ContentCanvas.CanCopyImageToClipboard();
        }

        public void CopyImageToClipboard()
        {
            _viewComponent.ContentCanvas.CopyImageToClipboard();
        }

    }
}
