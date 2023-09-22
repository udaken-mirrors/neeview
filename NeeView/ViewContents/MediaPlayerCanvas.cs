using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// MediaPlayerCanvas Base
    /// </summary>
    public abstract class MediaPlayerCanvas : Grid, IDisposable
    {
        private bool _disposedValue;

        public virtual void SetViewbox(Rect viewbox) { }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
