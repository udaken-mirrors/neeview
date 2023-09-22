using System;

namespace NeeView
{
    public class ExceptionEventArgs : EventArgs
    {
        public ExceptionEventArgs(Exception errorException)
        {
            ErrorException = errorException;
        }

        public Exception ErrorException { get; }
    }
}