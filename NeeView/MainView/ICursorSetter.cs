//#define LOCAL_DEBUG

using System.Windows.Input;

namespace NeeView
{
    public interface ICursorSetter
    {
        void SetCursor(Cursor? cursor);
    }
}
