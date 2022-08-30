using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// App.Current.Dispatcher.Invoke系のラッパー
    /// </summary>
    public static class AppDispatcher
    {
        public static Dispatcher UIDispatcher { get; } = App.Current.Dispatcher;


        public static void Invoke(Action action)
        {
            if (UIDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                UIDispatcher.Invoke(action);
            }
        }


        public static TResult Invoke<TResult>(Func<TResult> func)
        {
            if (UIDispatcher.CheckAccess())
            {
                return func.Invoke();
            }
            else
            {
                return UIDispatcher.Invoke(func);
            }
        }

        public static async Task InvokeAsync(Action action)
        {
            if (UIDispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                await UIDispatcher.InvokeAsync(action);
            }
        }

        public static void BeginInvoke(Action action)
        {
            if (UIDispatcher.CheckAccess())
            {
                action.Invoke();
            }
            else
            {
                UIDispatcher.BeginInvoke(action);
            }
        }

    }
}
