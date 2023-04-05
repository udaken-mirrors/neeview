using NeeView.Collections.Generic;
using NeeView.Windows.Media;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ListBoxItem用RenameControl定義
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class ListBoxItemRenameControl<TItem> : RenameControl
        where TItem : class, IRenameable
    {
        private readonly Window _window;
        private readonly Locker _scrollChangedLocker = new();
        protected ListBox _listBox;
        protected TItem _item;


        public ListBoxItemRenameControl(ListBox listBox, TItem item) : base(CreateRenameControlSource(listBox, item))
        {
            _window = Window.GetWindow(listBox);
            _listBox = listBox;
            _item = item;
            this.Loaded += (s, e) => OnLoaded();
            this.Unloaded += (s, e) => OnUnloaded();
        }

        private static RenameControlSource CreateRenameControlSource(ListBox listBox, TItem item)
        {
            listBox.ScrollIntoView(item);
            listBox.UpdateLayout();
            var listBoxItem = VisualTreeUtility.GetListBoxItemFromItem(listBox, item) ?? throw new InvalidOperationException("ListBoxItem not found.");
            var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(listBoxItem, "FileNameTextBlock") ?? throw new InvalidOperationException("TextBlock(FileNameTextBlock) not foud.");
            return new RenameControlSource(listBoxItem, textBlock, item.GetRenameText());
        }


        public event EventHandler<RenameControlStateChangedEventArgs>? StateChanged;


        protected override async Task<bool> OnRenameAsync(string oldValue, string newValue)
        {
            Debug.Assert(oldValue != newValue);
            return await _item.RenameAsync(newValue);
        }

        protected virtual void OnLoaded()
        {
            _window.SizeChanged += Window_SizeChanged;
            _listBox.SelectionChanged += ListBox_SelectionChanged;
            _listBox.Unloaded += ListBox_Unloaded;
            _listBox.AddHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)ListBox_ScrollChanged);
        }

        protected virtual void OnUnloaded()
        {
            _window.SizeChanged -= Window_SizeChanged;
            _listBox.SelectionChanged -= ListBox_SelectionChanged;
            _listBox.Unloaded -= ListBox_Unloaded;
            _listBox.RemoveHandler(ScrollViewer.ScrollChangedEvent, (ScrollChangedEventHandler)ListBox_ScrollChanged);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            using var locked = _scrollChangedLocker.Lock();
            _listBox.ScrollIntoView(_listBox.SelectedItem);
            _listBox.UpdateLayout();
            StateChanged?.Invoke(this, new RenameControlStateChangedEventArgs(RenameControlStateChangedAction.LayoutChanged));
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StateChanged?.Invoke(this, new RenameControlStateChangedEventArgs(RenameControlStateChangedAction.SelectionChanged));
        }

        private void ListBox_Unloaded(object sender, RoutedEventArgs e)
        {
            StateChanged?.Invoke(this, new RenameControlStateChangedEventArgs(RenameControlStateChangedAction.Unloaded));
        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollChangedLocker.IsLocked) return;
            if (e.VerticalChange != 0.0 || e.HorizontalChange != 0.0)
            {
                StateChanged?.Invoke(this, new RenameControlStateChangedEventArgs(RenameControlStateChangedAction.ScrollChanged));
            }
        }
    }
}
