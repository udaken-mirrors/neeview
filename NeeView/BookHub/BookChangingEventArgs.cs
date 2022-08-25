using System;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    public class BookChangingEventArgs : EventArgs
    {
        public BookChangingEventArgs(string address)
        {
            Address = address;
        }

        public string Address { get; set; }
    }
}

