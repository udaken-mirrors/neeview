using NeeView.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace NeeView
{
    public partial class App
    {
        // 未処理例外発生数
        private int _exceptionCount = 0;


        public event EventHandler? CriticalError;


        /// <summary>
        /// 全ての未処理例外をキャッチするハンドル登録
        /// </summary>
        private void InitializeUnhandledException()
        {
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }

        /// <summary>
        /// TaskScheduler の未処理例外処理
        /// </summary>
        private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            // OperationCanceledException は正常とする
            if (IsOperationCanceledException(e.Exception))
            {
                e.SetObserved();
            }
            else
            {
                // NOTE: .NET6 では ThrowUnobservedTaskExceptions が機能しないためここで例外を発行してアプリ終了させる
                throw e.Exception;
            }
        }

        /// <summary>
        /// Dispatcher の未処理例外処理
        /// </summary>
        private static void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // OperationCanceledException は正常とする
            if (IsOperationCanceledException(e.Exception))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 例外がOperationCanceledExceptionのみであるか
        /// </summary>
        private static bool IsOperationCanceledException(Exception exception)
        {
            if (exception is AggregateException aggregateException)
            {
                return aggregateException.InnerExceptions.All(x => x is OperationCanceledException);
            }
            else if (exception is OperationCanceledException)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// アプリドメインの未処理例外の処理。アプリは継続不能
        /// </summary>
        private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            int count = Interlocked.Increment(ref _exceptionCount);
            if (count >= 2) return;

            CriticalError?.Invoke(this, EventArgs.Empty);

            var exception = e.ExceptionObject as Exception;
            if (exception == null) return;

            string errorLog = CreateErrorLog(exception);

            // エラーログファイルに出力
            var errorLogFileName = System.IO.Path.Combine(Environment.LocalApplicationDataPath, "ErrorLog.txt");
            using (var writer = new StreamWriter(new FileStream(errorLogFileName, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(errorLog);
            }

            // エラーダイアログ表示
            ShowErrorLogDialog(errorLog, errorLogFileName);

            Debug.WriteLine("UnhandledException callback done.");
        }

        /// <summary>
        /// エラーログテキスト作成
        /// </summary>
        /// <param name="exception">例外</param>
        private string CreateErrorLog(Exception exception)
        {
            string errorLog;
            using (var writer = new StringWriter())
            {
                writer.WriteLine("OS Version: " + System.Environment.OSVersion + (Environment.IsX64 ? " (64bit)" : " (32bit)"));
                writer.WriteLine("NeeView Version: " + Environment.DispVersion + $" ({Environment.PackageType})");
                writer.WriteLine("");
                writer.WriteLine(exception.ToStackString());

                errorLog = writer.ToString();
            }

            return errorLog;
        }

        /// <summary>
        /// エラーダイアログ表示
        /// </summary>
        private void ShowErrorLogDialog(string errorLog, string errorLogFileName)
        {
            try
            {
                AppDispatcher.Invoke(() =>
                {
                    var dialog = new CriticalErrorDialog(errorLog, errorLogFileName);
                    dialog.ShowInTaskbar = true;
                    dialog.Topmost = true;
                    dialog.ShowDialog();
                });
            }
            catch
            {
                MessageBox.Show(errorLog, "Abort", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }
    }
}
