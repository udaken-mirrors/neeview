using System;

namespace NeeView
{
    /// <summary>
    /// Lazy拡張
    /// </summary>
    /// <remarks>
    /// Created イベント追加
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class LazyEx<T>
    {
        private readonly Lazy<T> _lazy;

        public LazyEx(Func<T> valueFactory)
        {
            _lazy = new(valueFactory);
        }

        public event EventHandler<LazyExCreatedEventArgs<T>>? Created;

        public bool IsValueCreated => _lazy.IsValueCreated;

        public T Value
        {
            get
            {
                if (_lazy.IsValueCreated)
                {
                    return _lazy.Value;
                }
                else
                {
                    var value = _lazy.Value;
                    Created?.Invoke(this, new LazyExCreatedEventArgs<T>(value));
                    return value;
                }
            }
        }
    }


    public class LazyExCreatedEventArgs<T> : EventArgs
    {
        public T Value { get; }

        public LazyExCreatedEventArgs(T value)
        {
            Value = value;
        }
    }
}
