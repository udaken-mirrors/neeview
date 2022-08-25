using System;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    public class BookHubPathEventArgs : EventArgs
    {
        public BookHubPathEventArgs(string? path)
        {
            Path = path;
        }

        public string? Path { get; set; }
    }
}

