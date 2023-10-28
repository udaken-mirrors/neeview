using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NeeView.Windows;
using System.Threading;
using NeeView.Data;

namespace NeeView
{
    public class FocusChangedEventArgs : EventArgs
    {
        public FocusChangedEventArgs(bool isFocused)
        {
            IsFocused = isFocused;
        }

        public bool IsFocused { get; set; }
    }


    /// <summary>
    /// FolderListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListView : UserControl, IHasFolderListBox
    {
        private readonly FolderListViewModel _vm;
        private int _requestSearchBoxFocusValue;


        public FolderListView(BookshelfFolderList model)
        {
            InitializeComponent();

            this.FolderTree.Model = new BookshelfFolderTreeModel(model);

            _vm = new FolderListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.SearchBoxFocus += FolderList_SearchBoxFocus;
            model.FolderTreeFocus += FolderList_FolderTreeFocus;

            this.SearchBox.IsKeyboardFocusedChanged +=
                (s, e) => SearchBoxFocusChanged?.Invoke(this, new FocusChangedEventArgs((bool)e.NewValue));
        }

        
        public event EventHandler<FocusChangedEventArgs>? SearchBoxFocusChanged;


        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
            _vm.Dpi = newDpi.DpiScaleX;
        }

        /// <summary>
        /// フォルダーツリーへのフォーカス要求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_FolderTreeFocus(object? sender, EventArgs e)
        {
            if (!_vm.Model.FolderListConfig.IsFolderTreeVisible) return;

            this.FolderTree.FocusSelectedItem();
        }

        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderList_SearchBoxFocus(object? sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref _requestSearchBoxFocusValue, 1) == 0)
            {
                _ = FocusSearchBoxAsync(); // 非同期
            }
        }

        /// <summary>
        /// 検索ボックスにフォーカスをあわせる。
        /// </summary>
        /// <returns></returns>
        private async Task FocusSearchBoxAsync()
        {
            // 表示が間に合わない場合があるので繰り返しトライする
            for (int i = 0; i < 10; i++)
            {
                var searchBox = this.SearchBox;
                if (searchBox != null && searchBox.IsLoaded && searchBox.IsVisible && this.IsVisible)
                {
                    searchBox.FocusEditableTextBox();
                    var isFocused = searchBox.IsKeyboardFocusWithin;
                    //Debug.WriteLine($"Focus: {isFocused}");
                    if (isFocused) break;
                }

                //Debug.WriteLine($"Focus: ready...");
                await Task.Delay(100);
            }

            Interlocked.Exchange(ref _requestSearchBoxFocusValue, 0);
            //Debug.WriteLine($"Focus: done.");
        }

        /// <summary>
        /// 履歴戻るボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderPrevButton_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(-1, 10);
        }

        /// <summary>
        /// 履歴進むボタンコンテキストメニュー開く 前処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderNextButton_ContextMenuOpening(object? sender, ContextMenuEventArgs e)
        {
            var menu = (sender as FrameworkElement)?.ContextMenu;
            if (menu == null) return;
            menu.ItemsSource = _vm.GetHistory(+1, 10);
        }

        private void FolderListView_IsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void Grid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
            {
                _vm.Model.AreaWidth = e.NewSize.Width;
            }
            if (e.HeightChanged)
            {
                _vm.Model.AreaHeight = e.NewSize.Height;
            }
        }

        #region DragDrop

        private readonly DragDropGhost _ghost = new();
        private bool _isButtonDown;
        private Point _buttonDownPos;

        private void PlaceIcon_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            var element = sender as UIElement;
            _buttonDownPos = e.GetPosition(element);
            _isButtonDown = true;
        }

        private void PlaceIcon_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            _isButtonDown = false;
        }

        private void PlaceIcon_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isButtonDown)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                _isButtonDown = false;
                return;
            }

            var element = sender as UIElement;

            var pos = e.GetPosition(element);
            if (DragDropHelper.IsDragDistance(pos, _buttonDownPos))
            {
                _isButtonDown = false;

                if (_vm.Model.Place == null)
                {
                    return;
                }
                if (_vm.Model.Place.Scheme != QueryScheme.File && _vm.Model.Place.Scheme != QueryScheme.Bookmark)
                {
                    return;
                }

                var data = new DataObject();
                data.SetData(new QueryPathCollection() { _vm.Model.Place });

                _ghost.Attach(this.PlaceBar, new Point(24, 24));
                DragDrop.DoDragDrop(element, data, DragDropEffects.Copy);
                _ghost.Detach();
            }
        }

        private void PlaceIcon_QueryContinueDrag(object? sender, QueryContinueDragEventArgs e)
        {
            _ghost.QueryContinueDrag(sender, e);
        }

        #endregion

        public void SetFolderListBoxContent(FolderListBox content)
        {
            this.ListBoxContent.Content = content;
        }

        // TODO: 共通化
        private void Root_KeyDown(object? sender, KeyEventArgs e)
        {
            // このパネルで使用するキーのイベントを止める
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Up || e.Key == Key.Down || (_vm.IsLRKeyEnabled() && (e.Key == Key.Left || e.Key == Key.Right)) || e.Key == Key.Return || e.Key == Key.Delete)
                {
                    e.Handled = true;
                }
            }
        }


        #region UI Accessor

        public void SetSearchBoxText(string text)
        {
            this.SearchBox.SetCurrentValue(SearchBox.TextProperty, text);
        }

        public string GetSearchBoxText()
        {
            return this.SearchBox.Text;
        }

        #endregion UI Accessor
    }

}
