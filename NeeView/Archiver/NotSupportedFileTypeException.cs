using System;
using System.Runtime.Serialization;

namespace NeeView
{
    public class NotSupportedFileTypeException : Exception
    {
        public NotSupportedFileTypeException(string extension) : base(string.Format(Properties.Resources.Notice_NotSupportedFileType, extension))
        {
            Extension = extension;
        }

        public NotSupportedFileTypeException(string extension, string message) : base(message)
        {
            Extension = extension;
        }

        public NotSupportedFileTypeException(string extension, string message, Exception inner) : base(message)
        {
            Extension = extension;
        }

        public string Extension { get; set; }
    }
}
