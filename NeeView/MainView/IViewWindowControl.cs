namespace NeeView
{
    public interface IViewWindowControl
    {
        void SetFullScreen(object? sender, bool isFullScreen);
        void StretchWindow();
        void ToggleTopmost(object? sender);
        void ToggleWindowFullScreen(object? sender);
        void ToggleWindowMaximize(object? sender);
        void ToggleWindowMinimize(object? sender);
    }
}
