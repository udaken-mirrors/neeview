using System;

namespace NeeView
{
    /// <summary>
    /// Susie 例外
    /// </summary>
    [Serializable]
    public class SusieIOException : Exception
    {
        public SusieIOException() : base(Properties.Resources.SusieLoadFailedException_Message)
        {
        }

        public SusieIOException(string message) : base(message)
        {
        }

        public SusieIOException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
