using System;

namespace NeeView.Windows
{
    public class WindowStateExChangedEventArgs : EventArgs
    {
        public WindowStateExChangedEventArgs(WindowStateEx oldState, WindowStateEx newtate)
        {
            OldState = oldState;
            NewState = newtate;
        }

        public WindowStateEx OldState { get; set; }
        public WindowStateEx NewState { get; set; }
    }
}
