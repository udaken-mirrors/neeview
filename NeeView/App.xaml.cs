﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private int _ExceptionCount = 0;

        public static string StartupPlace { get; set; }

        //
        public void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (++_ExceptionCount >= 2)
            {
                Debug.WriteLine($"AfterException({_ExceptionCount}): {e.Exception.Message}");
                e.Handled = true;
                return;
            }

            using (var stream = new FileStream("ErrorLog.txt", FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine($"{DateTime.Now}\n");

                Action<Exception, StreamWriter> WriteException = (exception, sw) =>
                {
                    sw.WriteLine($"ExceptionType:\n  {exception.GetType()}");
                    sw.WriteLine($"ExceptionMessage:\n  {exception.Message}");
                    sw.WriteLine($"ExceptionStackTrace:\n{exception.StackTrace}");
                };

                WriteException(e.Exception, writer);

                Exception ex = e.Exception.InnerException;
                while (ex != null)
                {
                    writer.WriteLine("\n\n-------- InnerException --------\n");
                    WriteException(ex, writer);
                    ex = ex.InnerException;
                }
            }

            string exceptionMessage = e.Exception is System.Reflection.TargetInvocationException ? e.Exception.InnerException?.Message : e.Exception.Message;
            string message = $"エラーが発生しました。アプリを終了します。\n\n理由 : {exceptionMessage}\n\nErrorLog.txtにエラーの詳細が出力されています。この内容を開発者に報告してください。";
            MessageBox.Show(message, "強制終了", MessageBoxButton.OK, MessageBoxImage.Error);

#if DEBUG
#else
            e.Handled = true;

            this.Shutdown();
#endif
        }

        //
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // カレントフォルダをアプリの場所に再設定
            var myAssembly = Assembly.GetEntryAssembly();
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName(myAssembly.Location);

            // 引数チェック
            foreach (string arg in e.Args)
            {
                StartupPlace = arg.Trim();
            }
        }
    }
}
