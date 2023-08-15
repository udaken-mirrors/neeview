using System;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// 条件をトリガーにした遅延実行
    /// </summary>
    public class ConditionalDelayAction : IDisposable
    {
        private Action? _action;
        private Func<bool>? _condition;
        private DispatcherTimer _timer;
        private bool _disposedValue;

        public ConditionalDelayAction() : this(TimeSpan.FromMilliseconds(16))
        {
        }

        public ConditionalDelayAction(TimeSpan interval)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = interval;
            _timer.Tick += Timer_Tick;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick; // 自己参照なので破棄しなくてもよいのだが
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        public bool IsBusy => _action is not null;


        public void Request(Action action, Func<bool> condition)
        {
            _action = action;
            _condition = condition;

            Invoke();

            if (IsBusy)
            {
                _timer.Start();
            }
        }

        public void Clear()
        {
            _action = null;
            _condition = null;
            _timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            Invoke();

            if (!IsBusy)
            {
                _timer.Stop();
            }
        }

        private void Invoke()
        {
            if (_action is null) return;
            if (_condition is null) return;

            if (_condition())
            {
                _action.Invoke();
                Clear();
            }
        }

    }
}
