﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// PageListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListControl : UserControl
    {
        public BookHub BookHub
        {
            get { return (BookHub)GetValue(BookHubProperty); }
            set { SetValue(BookHubProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BookHub.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookHubProperty =
            DependencyProperty.Register("BookHub", typeof(BookHub), typeof(PageListControl), new PropertyMetadata(null, new PropertyChangedCallback(BookHubPropertyChanged)));

        //
        public static void BookHubPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            PageListControl ctrl = d as PageListControl;
            if (ctrl != null)
            {
                ctrl._VM.BookHub = ctrl.BookHub;
            }
        }


        private PageListControlVM _VM;


        public PageListControl()
        {
            InitializeComponent();

            _VM = new PageListControlVM();
            _VM.PagesChanged += OnPagesChanged;
            this.DockPanel.DataContext = _VM;
        }

        private void OnPagesChanged(object sender, EventArgs e)
        {
            this.PageListBox.Items.Refresh();
            FocusSelectedItem();
        }

        //
        public void Initialize(BookHub bookHub)
        {
            _VM.Initialize(bookHub);
        }


        //
        public void FocusSelectedItem()
        {
            ListBoxItem lbi = (ListBoxItem)(this.PageListBox.ItemContainerGenerator.ContainerFromIndex(this.PageListBox.SelectedIndex));
            lbi?.Focus();
        }

        // フォルダリスト 選択項目変更
        private void PageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.IsLoaded)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
                //SelectionChanged?.Invoke(this, listBox.SelectedIndex);
            }
        }


        // 履歴項目決定
        private void PageListItem_MouseSingleClick(object sender, MouseButtonEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            if (page != null)
            {
                _VM.Jump(page);
                e.Handled = true;
            }
        }

        // 履歴項目決定(キー)
        private void PageListItem_KeyDown(object sender, KeyEventArgs e)
        {
            var page = (sender as ListBoxItem)?.Content as Page;
            {
                if (e.Key == Key.Return)
                {
                    _VM.Jump(page);
                    e.Handled = true;
                }
            }
        }

        // リストのキ入力
        private void PageList_KeyDown(object sender, KeyEventArgs e)
        {
            // このパネルで使用するキーのイベントを止める
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Return || e.Key == Key.Delete)
            {
                e.Handled = true;
            }
        }
    }

    public enum PageNameFormat
    {
        None,
        Smart,
        NameOnly,
    }

    /// <summary>
    /// 
    /// </summary>
    public class PageListControlVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public event EventHandler PagesChanged;

        private BookHub _BookHub;
        public BookHub BookHub
        {
            get { return _BookHub; }
            set
            {
                _BookHub = value;
                _BookHub.BookChanged += (s, e) => Reflesh();
                _BookHub.PagesSorted += (s, e) => Reflesh();
                _BookHub.ViewContentsChanged += BookHub_ViewContentsChanged;
                OnPropertyChanged();
                Reflesh();
            }
        }

        private void BookHub_ViewContentsChanged(object sender, ViewSource e)
        {
            var contents = e?.Sources;
            if (contents == null) return;

            var mainContent = contents.Count > 0 ? (contents.First().Position < contents.Last().Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SelectedItem = mainContent.Page;
            }
        }

        public Dictionary<PageNameFormat, string> FormatList { get; } = new Dictionary<PageNameFormat, string>
        {
            [PageNameFormat.None] = "そのまま",
            [PageNameFormat.Smart] = "標準表示",
            [PageNameFormat.NameOnly] = "名前のみ",
        };

        #region Property: Format
        private PageNameFormat _Format = PageNameFormat.Smart;
        public PageNameFormat Format
        {
            get { return _Format; }
            set { _Format = value; OnPropertyChanged(); }
        }
        #endregion



        public Dictionary<PageSortMode, string> PageSortModeList => PageSortModeExtension.PageSortModeList;

        #region Property: Title
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { _Title = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: PageSortMode
        private PageSortMode _PageSortMode;
        public PageSortMode PageSortMode
        {
            get { return _PageSortMode; }
            set { _PageSortMode = value; _BookHub.SetSortMode(value); }
        }
        #endregion



        #region Property: Pages
        private List<Page> _Pages;
        public List<Page> Pages
        {
            get { return _Pages; }
            set { _Pages = value; OnPropertyChanged(); }
        }
        #endregion

        #region Property: SelectedItem
        private Page _SelectedItem;
        public Page SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; OnPropertyChanged(); }
        }
        #endregion

        //
        private void Reflesh()
        {
            Title = System.IO.Path.GetFileName(_BookHub.CurrentBook?.Place);

            _PageSortMode = _BookHub.BookMemento.SortMode;
            OnPropertyChanged(nameof(PageSortMode));

            Pages = _BookHub.CurrentBook?.Pages;
            App.Current.Dispatcher.Invoke(() => PagesChanged?.Invoke(this, null));
        }

        //
        public void Initialize(BookHub bookHub)
        {
            BookHub = bookHub;
        }

        //
        public void Jump(Page page)
        {
            _BookHub.JumpPage(page);
        }

    }
}
