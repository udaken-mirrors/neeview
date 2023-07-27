using System;
using System.Diagnostics;

namespace NeeView
{
    public class BookPageTerminatorProxy : IDisposable
    {
        private BookPageTerminator? _source;
        private bool _disposedValue;

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

        public void SetSource(BookPageTerminator? source)
        {
            if (_source == source) return;

            Detach();
            Attach(source);
        }

        public void Attach(BookPageTerminator? source)
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

}
