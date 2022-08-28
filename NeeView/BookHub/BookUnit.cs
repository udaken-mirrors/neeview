using System;
using System.Diagnostics;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    /// <summary>
    /// Bookとその付属情報の管理
    /// </summary>
    public class BookUnit : IDisposable
    {
        public BookUnit(Book book, BookAddress bookAddress, BookLoadOption loadOptions)
        {
            Debug.Assert(book != null);
            Book = book;
            BookAddress = bookAddress;
            LoadOptions = loadOptions;
        }

        public Book Book { get; private set; }
        public BookAddress BookAddress { get; private set; }
        public BookLoadOption LoadOptions { get; private set; }

        public bool IsKeepHistoryOrder
            => (LoadOptions & BookLoadOption.KeepHistoryOrder) == BookLoadOption.KeepHistoryOrder;

        public bool IsValid
            => Book?.Address != null;


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Book.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public BookMementoType GetBookMementoType()
        {
            if (Book is null) return BookMementoType.None;

            if (BookmarkCollection.Current.Contains(Book.Address))
            {
                return BookMementoType.Bookmark;
            }
            else if (BookHistoryCollection.Current.Contains(Book.Address))
            {
                return BookMementoType.History;
            }
            else
            {
                return BookMementoType.None;
            }
        }
    }
}

