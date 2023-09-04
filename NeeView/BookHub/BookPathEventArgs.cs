using System;


namespace NeeView
{
    public class BookPathEventArgs : EventArgs
    {
        public BookPathEventArgs(string? path)
        {
            Path = path;
        }

        public string? Path { get; set; }
    }
}

