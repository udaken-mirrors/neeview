﻿using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace NeeView.Setting
{
    /// <summary>
    /// SettingMouseDragControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingMouseDragControl : UserControl
    {
        public SettingMouseDragControl()
        {
            InitializeComponent();

            Initialize();
            this.Root.DataContext = this;
        }


        public void Initialize()
        {
            // ドラッグアクション一覧作成
            DragActionCollection = new ObservableCollection<DragActionParam>();
            UpdateDragActionList();
        }

        // ドラッグ一覧専用パラメータ
        public class DragActionParam : BindableBase
        {
            public DragActionType Key { get; set; }
            public DragAction DragAction { get; set; }

            public string Header => Key.ToAliasName();
            public string Tips => Key.ToTips();
        }

        // コマンド一覧
        public ObservableCollection<DragActionParam> DragActionCollection { get; set; }

        //
        private Window GetOwner()
        {
            return Window.GetWindow(this);
        }

        //
        private void UpdateDragActionList()
        {
            DragActionCollection.Clear();
            foreach (var element in DragActionTable.Current)
            {
                var item = new DragActionParam()
                {
                    Key = element.Key,
                    DragAction = element.Value,
                };

                DragActionCollection.Add(item);
            }

            this.DragActionListView.Items.Refresh();
        }

        //
        private void DragActionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // nop.
        }

        //
        private void DragActionListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem targetItem = (ListViewItem)sender;

            var value = (DragActionParam)targetItem.DataContext;
            OpenDragActionSettingDialog(value);
        }

        //
        private void DragActionSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (DragActionParam)this.DragActionListView.SelectedValue;
            OpenDragActionSettingDialog(value);
        }

        //
        private void OpenDragActionSettingDialog(DragActionParam value)
        {
            if (value.DragAction.IsLocked)
            {
                var dlg = new MessageDialog("", "この操作は変更できません");
                dlg.Owner = GetOwner();
                dlg.ShowDialog();
                return;
            }

            var dialog = new MouseDragSettingWindow();
            dialog.Initialize(value.Key);
            dialog.Owner = GetOwner();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();

            if (result == true)
            {
                this.DragActionListView.Items.Refresh();
            }
        }

        //
        private void ResetDragActionSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog($"すべてのドラッグ操作を初期化します。よろしいですか？", "ドラッグ操作を初期化します");
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);
            dialog.Owner = GetOwner();
            var answer = dialog.ShowDialog();

            if (answer == UICommands.Yes)
            {
                var memento = DragActionTable.CreateDefaultMemento();
                DragActionTable.Current.Restore(memento);

                this.DragActionListView.Items.Refresh();
            }
        }
    }
}