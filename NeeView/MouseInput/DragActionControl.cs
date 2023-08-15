using System;
using System.Windows.Input;

namespace NeeView
{

    public class DragActionControl : IDisposable
    {
        private DragAction _source;
        private bool _disposedValue;

        public DragActionControl(DragTransformContext context, DragAction source)
        {
            Context = context;
            _source = source;
        }

        public DragTransformContext Context { get; }

        public DragKey DragKey => _source.DragKey;

        public DragActionParameter? Parameter => _source.Parameter;



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

        public virtual void ExecuteBegin()
        {
        }

        public virtual void Execute()
        {
        }

        public virtual void ExecuteEnd(bool continued)
        {
        }

        public virtual void MouseWheel(MouseWheelEventArgs e)
        {
        }

    }



}