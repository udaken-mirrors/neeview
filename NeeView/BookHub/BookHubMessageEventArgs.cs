using System;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    public class BookHubMessageEventArgs : EventArgs
    {
        public BookHubMessageEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; set; }
    }
}

