namespace NeeView
{
    public interface IViewAutoScrollControl
    {
        bool GetAutoScrollMode();
        void SetAutoScrollMode(bool isAutoScroll);
        void ToggleAutoScrollMode();
    }
}