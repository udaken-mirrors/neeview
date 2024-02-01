using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// 一度だけ実行される同期処理
    /// </summary>
    /// <remarks>
    /// 同じフレームに複数のイベントで重複処理されてしまう等を防ぐ
    /// </remarks>
    public class OnceDispatcher : IDisposable
    {
        private Dictionary<object, DispatcherOperation> _map = new();
        private object _lock = new object();
        private bool _disposedValue;


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Clear();
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
        /// 同期処理を一度だけ実行する
        /// </summary>
        /// <param name="key">識別キー。おなじキーの処理はキャンセルされる</param>
        /// <param name="action">実行する処理</param>
        public void BeginInvoke(object key, Action action)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(key, out var operation))
                {
                    operation.Abort();
                }

                _map[key] = AppDispatcher.BeginInvoke(() => DispatchAction(key, action));
            }
        }

        private void DispatchAction(object key, Action action)
        {
            action();
            lock (_lock)
            {
                _map.Remove(key);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                foreach (var operation in _map.Values)
                {
                    operation.Abort();
                }
                _map.Clear();
            }
        }

    }
}
