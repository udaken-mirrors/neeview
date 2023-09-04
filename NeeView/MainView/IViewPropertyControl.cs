using System.ComponentModel;

namespace NeeView
{
    public interface IViewPropertyControl : INotifyPropertyChanged
    {
        bool IsAutoRotateLeft { get; set; }
        bool IsAutoRotateRight { get; set; }

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
}