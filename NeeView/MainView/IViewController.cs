namespace NeeView
{
    public interface IViewController : IViewTransformControl, IViewPropertyControl, IViewLoupeControl, IWindowControl, IBookCopyImage, IBookPrint
    {
        void OpenContextMenu();
        void TouchInputEmutrate(object? sender);
    }


    public interface IBookCopyImage
    {
        bool CanCopyImageToClipboard();
        void CopyImageToClipboard();
    }

    public interface IBookPrint
    {
        bool CanPrint();
        void Print();
    }

    public interface IWindowControl
    {
        void SetFullScreen(object? sender, bool isFullScreen);
        void StretchWindow();
        void ToggleTopmost(object? sender);
        void ToggleWindowFullScreen(object? sender);
        void ToggleWindowMaximize(object? sender);
        void ToggleWindowMinimize(object? sender);
    }

    public interface IViewLoupeControl
    {
        bool GetLoupeMode();
        void SetLoupeMode(bool isLoupeMode);
        void ToggleLoupeMode();
        void LoupeZoomIn();
        void LoupeZoomOut();
    }

    public interface IViewPropertyControl
    {
        bool GetAutoRotateLeft();
        bool GetAutoRotateRight();
        void SetAutoRotateLeft(bool flag);
        void SetAutoRotateRight(bool flag);
        void ToggleAutoRotateLeft();
        void ToggleAutoRotateRight();

        PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter parameter);
        PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter parameter);
        void SetStretchMode(PageStretchMode mode, bool isToggle);
        bool TestStretchMode(PageStretchMode mode, bool isToggle);
    }

    public interface IViewTransformControl
    {
        void ResetContentSizeAndTransform();

        void ScaleUp(ViewScaleCommandParameter parameter);
        void ScaleDown(ViewScaleCommandParameter parameter);

        void ViewRotateLeft(ViewRotateCommandParameter parameter);
        void ViewRotateRight(ViewRotateCommandParameter parameter);

        void FlipHorizontal(bool isFlip);
        void FlipVertical(bool isFlip);
        void ToggleFlipHorizontal();
        void ToggleFlipVertical();

        void ScrollUp(ViewScrollCommandParameter parameter);
        void ScrollDown(ViewScrollCommandParameter parameter);
        void ScrollLeft(ViewScrollCommandParameter parameter);
        void ScrollRight(ViewScrollCommandParameter parameter);

        void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter);
        void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter);


        //[Obsolete("use IBookPageMoveControl.ScrollToNextPage()")]
        void NextScrollPage(object? sender, ScrollPageCommandParameter parameter);
        //[Obsolete("use IBookPageMoveControl.ScrollToPrevPage()")]
        void PrevScrollPage(object? sender, ScrollPageCommandParameter parameter);
    }
}