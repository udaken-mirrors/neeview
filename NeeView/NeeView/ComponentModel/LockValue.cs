using NeeLaboratory.ComponentModel;
using System;

namespace NeeView.ComponentModel
{
    public class LockValue<T>
    {
        private T _value;
        private bool _lock;

        public LockValue(T value)
        {
            _value = value;
        }

        public T Value
        {
            get { return _value; }
            set { Set(value); }
        }

        private void Set(T value)
        {
            if (_lock) return;
            _value = value;
        }

        public IDisposable Lock()
        {
            _lock = true;
            return new AnonymousDisposable(() => _lock = false);
        }
    }


}
