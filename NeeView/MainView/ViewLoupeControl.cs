namespace NeeView
{
#warning not support yet
    public class ViewLoupeControl: IViewLoupeControl
    {
        private readonly MainViewComponent _viewComponent;

        public ViewLoupeControl(MainViewComponent viewComponent)
        {
            _viewComponent = viewComponent;
        }

        public void SetLoupeMode(bool isLoupeMode)
        {
            // NOTE: 有効にするのはマウスのルーペモードのみ
            if (isLoupeMode)
            {
                _viewComponent.MouseInput.SetState(MouseInputState.Loupe, false);
            }
            else
            {
                //_viewComponent.LoupeTransform.IsEnabled = false;
            }
        }

        public bool GetLoupeMode()
        {
            return false;
            //return _viewComponent.LoupeTransform.IsEnabled;
        }

        public void ToggleLoupeMode()
        {
            SetLoupeMode(!GetLoupeMode());
        }

        public void LoupeZoomIn()
        {
            //_viewComponent.LoupeTransform.ZoomIn();
        }

        public void LoupeZoomOut()
        {
            //_viewComponent.LoupeTransform.ZoomOut();
        }

    }
}
