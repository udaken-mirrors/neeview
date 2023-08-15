using NeeLaboratory.Threading.Jobs;
using System;
using System.Diagnostics;
using System.Threading;

namespace NeeView
{
    public class BookPageScriptProxy : IDisposable
    {
        private BookPageScript? _source;
        private bool _disposedValue;

        public BookPageScriptProxy()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Detach();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetSource(BookPageScript? source)
        {
            if (_source == source) return;
            
            Detach();
            Attach(source);
        }

        public void Attach(BookPageScript? source)
        {
            Debug.Assert(_source is null);

            _source = source;
        }

        public void Detach()
        {
            if (_source is null) return;

            _source.Dispose();
            _source = null;
        }

    }


    public class BookPageScript : IDisposable
    {
        private Book Book;
        private bool _disposedValue;

        public BookPageScript(Book book)
        {
            Book = book;
            //Book.Viewer.Loader.ViewContentsChanged += Book_ViewContentsChanged;
            OnLoaded();
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    //Book.Viewer.Loader.ViewContentsChanged -= Book_ViewContentsChanged;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 本の更新
        /// </summary>
        private void OnLoaded()
        {
            AppDispatcher.Invoke(() =>
            {
                // Script: OnBookLoaded
                CommandTable.Current.TryExecute(this, ScriptCommand.EventOnBookLoaded, null, CommandOption.None);
                // Script: OnPageChanged
                CommandTable.Current.TryExecute(this, ScriptCommand.EventOnPageChanged, null, CommandOption.None);
            });
        }

#warning ページ変更時イベント未実装
#if false
        private void Book_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (e is null) return;
            if (!e.ViewPageCollection.IsFixedContents()) return;

            AppDispatcher.Invoke(() =>
            {
                // Script: OnPageChanged
                CommandTable.Current.TryExecute(this, ScriptCommand.EventOnPageChanged, null, CommandOption.None);
            });
        }
#endif
    }

}
