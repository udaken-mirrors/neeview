using System;
using System.Windows;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// 遅延実行
    /// 都度アクションと遅延時間を要求するタイプ
    /// </summary>
    public class InstantDelayAction : IDisposable
    {
        private Action? _action;
        private bool _disposedValue;
        private readonly DispatcherTimer _timer;
        private readonly object _lock = new();


        public InstantDelayAction() : this(Application.Current.Dispatcher)
        {
        }

        public InstantDelayAction(Dispatcher dispatcher)
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Tick += Timer_Tick;
        }

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
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public void Request(Action action, TimeSpan delay)
        {
            lock (_lock)
            {
                Cancel();

                _action = action ?? throw new ArgumentNullException(nameof(action));

                if (TimeSpan.Zero < delay)
                {
                    _timer.Interval = delay;
                    _timer.Start();
                }
                else
                {
                    Flush();
                }
            }
        }

        public bool Cancel()
        {
            lock (_lock)
            {
                _timer.Stop();

                if (_action is not null)
                {
                    _action = null;
                    return true;
                }

                return false;
            }
        }

        public bool Flush()
        {
            lock (_lock)
            {
                _timer.Stop();

                if (_action is not null)
                {
                    _action.Invoke();
                    _action = null;
                    return true;
                }

                return false;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Flush();
        }

    }
}
