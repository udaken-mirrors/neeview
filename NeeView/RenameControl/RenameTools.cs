using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public static class RenameTools
    {
        /// <summary>
        /// ListBoxスクロールのリネーム処理追従
        /// </summary>
        public static void ListBoxScrollChanged(ListBox listBox, ScrollChangedEventArgs e, RenameControl rename)
        {
            if (listBox is null) throw new ArgumentNullException(nameof(listBox));
            if (rename is null) return;

            if (e.VerticalChange != 0.0 || e.HorizontalChange != 0.0)
            {
                // リネームキャンセル
                rename.Close(true);
            }
            else
            {
                // リネームコントロール座標調整
                listBox.ScrollIntoView(listBox.SelectedItem);
                listBox.UpdateLayout();
                rename.SyncLayout();
            }
        }
    }
}
