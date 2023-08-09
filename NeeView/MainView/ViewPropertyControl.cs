namespace NeeView
{
    public class ViewPropertyControl : IViewPropertyControl
    {
        private readonly MainViewComponent _viewComponent;

        public ViewPropertyControl(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
        }

        public bool TestStretchMode(PageStretchMode mode, bool isToggle)
        {
            return _viewComponent.ContentCanvas.TestStretchMode(mode, isToggle);
        }

        public void SetStretchMode(PageStretchMode mode, bool isToggle)
        {
            _viewComponent.ContentCanvas.SetStretchMode(mode, isToggle);
        }

        public PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter parameter)
        {
            return _viewComponent.ContentCanvas.GetToggleStretchMode(parameter);
        }

        public PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter parameter)
        {
            return _viewComponent.ContentCanvas.GetToggleStretchModeReverse(parameter);
        }

        public bool GetAutoRotateLeft()
        {
            return _viewComponent.ContentCanvas.IsAutoRotateLeft;
        }

        public void SetAutoRotateLeft(bool flag)
        {
            _viewComponent.ContentCanvas.IsAutoRotateLeft = flag;
        }

        public void ToggleAutoRotateLeft()
        {
            _viewComponent.ContentCanvas.IsAutoRotateLeft = !_viewComponent.ContentCanvas.IsAutoRotateLeft;
        }

        public bool GetAutoRotateRight()
        {
            return _viewComponent.ContentCanvas.IsAutoRotateRight;
        }

        public void SetAutoRotateRight(bool flag)
        {
            _viewComponent.ContentCanvas.IsAutoRotateRight = flag;
        }

        public void ToggleAutoRotateRight()
        {
            _viewComponent.ContentCanvas.IsAutoRotateRight = !_viewComponent.ContentCanvas.IsAutoRotateRight;
        }
    }
}
