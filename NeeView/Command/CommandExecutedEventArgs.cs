using System;
using System.Windows.Input;

namespace NeeView
{
    public class CommandExecutedEventArgs : EventArgs
    {
        public CommandExecutedEventArgs(InputGesture gesture)
        {
            Gesture = gesture;
        }

        public InputGesture Gesture { get; set; }
    }
}
