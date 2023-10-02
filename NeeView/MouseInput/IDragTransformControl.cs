using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public interface IDragTransformControl
    {
        void ResetState();
        void UpdateState(MouseButtonBits buttons, ModifierKeys keys, Point point, int timestamp, ISpeedometer? speedometer, DragActionUpdateOptions options);
        void MouseWheel(MouseButtonBits buttons, ModifierKeys keys, MouseWheelEventArgs e);
    }
}
