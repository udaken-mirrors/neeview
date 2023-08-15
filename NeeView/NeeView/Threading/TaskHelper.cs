using System;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NeeView.Threading
{
    // from https://stackoverflow.com/questions/4331262/task-continuation-on-ui-thread
    public static class TaskHelper
    {
        /// <summary>
        /// UIスレッドに戻すAwaiter
        /// </summary>
        /// <remarks>
        /// トリッキーな動作であるため、非推奨とする.
        /// </remarks>
        [Obsolete]
        public static DispatcherAwaiter DispatcherAwaiter { get; } = new DispatcherAwaiter();
    }

    [Obsolete]
    public struct DispatcherAwaiter : INotifyCompletion
    {
        public bool IsCompleted => Application.Current.Dispatcher.CheckAccess();

        public void OnCompleted(Action continuation) => Application.Current.Dispatcher.Invoke(continuation);

        public void GetResult() { }

        public DispatcherAwaiter GetAwaiter()
        {
            return this;
        }
    }
}
