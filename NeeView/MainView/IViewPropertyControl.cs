using System.ComponentModel;

namespace NeeView
{
    public interface IViewPropertyControl : INotifyPropertyChanged
    {
        bool IsAutoRotateLeft { get; set; }
        bool IsAutoRotateRight { get; set; }
        bool IsAutoRotateForcedLeft { get; set; }
        bool IsAutoRotateForcedRight { get; set; }

        bool GetAutoRotateLeft();
        bool GetAutoRotateRight();
        bool GetAutoRotateForcedLeft();
        bool GetAutoRotateForcedRight();
        void SetAutoRotateLeft(bool flag);
        void SetAutoRotateRight(bool flag);
        void SetAutoRotateForcedLeft(bool flag);
        void SetAutoRotateForcedRight(bool flag);
        void ToggleAutoRotateLeft();
        void ToggleAutoRotateRight();
        void ToggleAutoRotateForcedLeft();
        void ToggleAutoRotateForcedRight();

        PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter parameter);
        PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter parameter);
        void SetStretchMode(PageStretchMode mode, bool isToggle);
        bool TestStretchMode(PageStretchMode mode, bool isToggle);
    }
}