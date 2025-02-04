﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Windows
{
    /// <summary>
    /// 複数選択専用ListBix
    /// </summary>
    public class ListBoxExtended : ListBox
    {
        public ListBoxExtended()
        {
            SelectionMode = SelectionMode.Extended;

            this.IsKeyboardFocusWithinChanged += ListBoxExtended_IsKeyboardFocusWithinChanged;
        }

        public event EventHandler<MouseButtonEventArgs>? PreviewMouseUpWithSelectionChanged;

        private void ListBoxExtended_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AppDispatcher.BeginInvoke(() => FocusSelectedItem(false));
        }

        public void FocusSelectedItem(bool force)
        {
            FocusItem(this.SelectedItem, force);
        }

        private void FocusItem(object? item, bool force)
        {
            if (item is null) return;

            var listBoxItem = this.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
            if (listBoxItem is not null)
            {
                if (force || this.IsKeyboardFocusWithin)
                {
                    listBoxItem.Focus();
                }
                this.SetAnchorItem(item);
            }
        }

        public void SetAnchorItem(object? item)
        {
            try
            {
                this.AnchorItem = item;
            }
            catch
            {
                // コンテナが生成されていないときに例外になるが、大きな影響はないので無視する
            }
        }

        public void ScrollSelectedItemsIntoView()
        {
            ScrollItemsIntoView(this.SelectedItems.Cast<object>());
        }

        public void ScrollItemsIntoView<T>(IEnumerable<T>? items)
        {
            if (items == null || !items.Any()) return;

            var top = items.First();

            // なるべく選択範囲が表示されるようにスクロールする
            this.ScrollIntoView(items.Last());
            this.UpdateLayout();
            this.ScrollIntoView(top);
            this.UpdateLayout();

            FocusItem(top, false);
        }

        public void SetSelectedItems<T>(IEnumerable<T>? newItems)
        {
            base.SetSelectedItems(newItems);
        }

        public void RaisePreviewMouseUpWithSelectionChanged(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseUpWithSelectionChanged?.Invoke(sender, e);
        }
    }
}
