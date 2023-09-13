using System;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    public class BookChangedEventArgs : EventArgs
    {
        public BookChangedEventArgs(string? address, Book? book, BookMementoType type)
        {
            Address = address;
            Book = book;
            BookMementoType = type;
        }

        public string? Address { get; set; }
        public Book? Book { get; set; }
        public BookMementoType BookMementoType { get; set; }

        public string? EmptyMessage { get; set; }
    }
}

