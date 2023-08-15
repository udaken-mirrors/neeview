using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// 遅延実行
    /// コマンドを遅延実行する。遅延中に要求された場合は古いコマンドをキャンセルする。
    /// </summary>
    public class DelayAction : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private readonly Action _action;
        private readonly object _lock = new();


        public DelayAction(Action action, TimeSpan delay) : this(action, delay, Application.Current.Dispatcher)
        {
        }

        public DelayAction(Action action, TimeSpan delay, Dispatcher dispatcher)
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Interval = delay;
            _timer.Tick += new EventHandler(DispatcherTimer_Tick);
            _action = action;
        }


        /// <summary>
        /// 実行要求
        /// </summary>
        public void Request()
        {
            if (_disposedValue) return;

            StartTimer();
        }

        /// <summary>
        /// 実行キャンセル
        /// </summary>
        public void Cancel()
        {
            StopTimer();
        }

        /// <summary>
        /// 遅延されている命令を即時実行する
        /// </summary>
        public void Flush()
        {
            if (_disposedValue) return;

            if (StopTimer())
            {
                _action.Invoke();
            }
        }

        private void StartTimer()
        {
            lock (_lock)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private bool StopTimer()
        {
            lock (_lock)
            {
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// timer callback
        /// </summary>
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            Flush();
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Cancel();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

}
