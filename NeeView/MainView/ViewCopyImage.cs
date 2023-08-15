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
#warning 未実装
            return false;
            //return _viewComponent.ContentCanvas.CanCopyImageToClipboard();
        }

        public void CopyImageToClipboard()
        {
#warning 未実装
            //_viewComponent.ContentCanvas.CopyImageToClipboard();
        }

    }
}
