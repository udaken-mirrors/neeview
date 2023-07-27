using System;


namespace NeeView
{
    public class BookHubPathEventArgs : EventArgs
    {
        public BookHubPathEventArgs(string? path)
        {
            Path = path;
        }

        public string? Path { get; set; }

        public bool IsLoading => Path != null;
    }
}

