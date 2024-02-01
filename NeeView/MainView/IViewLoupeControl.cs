namespace NeeView
{
    public interface IViewLoupeControl
    {
        bool GetLoupeMode();
        void SetLoupeMode(bool isLoupeMode);
        void ToggleLoupeMode();
        void LoupeZoomIn();
        void LoupeZoomOut();
    }
}
