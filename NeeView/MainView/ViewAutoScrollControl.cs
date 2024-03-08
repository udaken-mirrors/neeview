namespace NeeView
{
    public class ViewAutoScrollControl : IViewAutoScrollControl
    {
        private readonly MainViewComponent _viewComponent;

        public ViewAutoScrollControl(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
        }

        public void SetAutoScrollMode(bool isAutoScroll)
        {
            if (isAutoScroll)
            {
                _viewComponent.MouseInput.SetState(MouseInputState.AutoScroll, false);
            }
            else
            {
                if (_viewComponent.MouseInput.State == MouseInputState.AutoScroll)
                {
                    _viewComponent.MouseInput.ResetState();
                }
            }
        }

        public bool GetAutoScrollMode()
        {
            return _viewComponent.MouseInput.State == MouseInputState.AutoScroll;
        }

        public void ToggleAutoScrollMode()
        {
            SetAutoScrollMode(!GetAutoScrollMode());
        }

    }

}
