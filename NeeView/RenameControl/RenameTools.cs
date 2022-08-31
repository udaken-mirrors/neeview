using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public static class RenameTools
    {
        /// <summary>
        ///  要素の所属するウィンドウの RenameManager を取得する
        /// </summary>
        /// <param name="element">要素</param>
        /// <returns>RenameManager</returns>
        public static RenameManager GetRenameManager(UIElement element)
        {
            RenameManager? renameMabager = null;

            var window = Window.GetWindow(element);
            if (window is IHasRenameManager hasRenameManager)
            {
                renameMabager = hasRenameManager.GetRenameManager();
            }

            Debug.Assert(renameMabager != null);
            return renameMabager;
        }

        /// <summary>
        /// リネームコントロールを閉じたあとにフォーカスを戻す
        /// </summary>
        /// <param name="element">フォーカス要素</param>
        /// <param name="isFocused">リネームコントロールにフォーカスがあるか</param>
        /// <returns>フォーカスされたか</returns>
        public static bool RestoreFocus(UIElement element, bool isFocused)
        {
            if (element is null) return false;
            if (!isFocused) return false;

            return FocusTools.FocusIfWindowActived(element);
        }

        /// <summary>
        /// ListBoxスクロールのリネーム処理追従
        /// </summary>
        public static void ListBoxScrollChanged(ListBox listBox, ScrollChangedEventArgs e)
        {
            if (listBox is null) throw new ArgumentNullException(nameof(listBox));

            var renameManager = GetRenameManager(listBox);
            if (!renameManager.IsRenaming) return;

            if (e.VerticalChange != 0.0 || e.HorizontalChange != 0.0)
            {
                // リネームキャンセル
                renameManager.Stop();
            }
            else
            {
                // リネームコントロール座標調整
                listBox.ScrollIntoView(listBox.SelectedItem);
                listBox.UpdateLayout();
                renameManager.SyncLayout();
            }
        }
    }
}
