using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// アプリ終了時のデータ保存前に非同期動作しているサービスを停止させる仕組み。
    /// 事前にDisposableなサービスを登録しておく。
    /// </summary>
    public class ApplicationDisposer : IDisposable
    {
        static ApplicationDisposer() => Current = new ApplicationDisposer();
        public static ApplicationDisposer Current { get; }


        private List<IDisposable> _disposables = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            if (disposable is null) throw new ArgumentNullException(nameof(disposable));
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);

            _disposables.Add(disposable);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach(var disposable in _disposables.Reverse<IDisposable>())
                    {
                        disposable.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
