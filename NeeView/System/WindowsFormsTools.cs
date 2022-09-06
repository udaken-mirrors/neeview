using System;
using System.Windows;
using System.Windows.Interop;

// TODO: 要整備。表示やフロー等も含まれてしまっている。依存関係が強すぎる

namespace NeeView
{
    public static class WindowsFormsTools
    {
        public class Win32Window : System.Windows.Forms.IWin32Window
        {
            public IntPtr Handle { get; private set; }

            public Win32Window(Window window)
            {
                this.Handle = new WindowInteropHelper(window).Handle;
            }
        }


        /// <summary>
        /// フォルダー選択ダイアログ
        /// </summary>
        public static string? OpenFolderBrowserDialog(Window owner, string description)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = description;

            var result = dialog.ShowDialog(new Win32Window(owner));
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return dialog.SelectedPath;
            }
            else
            {
                return null;
            }
        }
    }
}
