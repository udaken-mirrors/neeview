using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public interface IDragTransformControl
    {
        void ResetState();
        void UpdateState(MouseButtonBits buttons, ModifierKeys keys, Point point, int timestamp);
    }
}
