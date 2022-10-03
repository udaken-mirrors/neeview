using System;
using System.Windows.Threading;

namespace NeeView.Threading
{
    /// <summary>
    /// 遅延実行
    /// 都度アクションと遅延時間を要求するタイプ
    /// </summary>
    public class InstantDelayAction
    {
        private Action? _action;
        private readonly DispatcherTimer _timer;
        private readonly object _lock = new();


        public InstantDelayAction() : this(App.Current.Dispatcher)
        {
        }

        public InstantDelayAction(Dispatcher dispatcher)
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, dispatcher);
            _timer.Tick += Timer_Tick;
        }


        public void Request(Action action, TimeSpan delay)
        {
            lock (_lock)
            {
                Cancel();

                _action = action ?? throw new ArgumentNullException(nameof(action));
                _timer.Interval = delay;
                _timer.Start();
            }
        }

        public void Cancel()
        {
            lock (_lock)
            {
                _timer.Stop();
                _action = null;
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
