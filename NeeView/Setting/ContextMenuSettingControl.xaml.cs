using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// ContextMenuSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ContextMenuSettingControl : UserControl
    {
        public static readonly RoutedCommand AddCommand = new("AddCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand RemoveCommand = new("RemoveCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand RenameCommand = new("RenameCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand MoveUpCommand = new("MoveUpCommand", typeof(ContextMenuSettingControl));
        public static readonly RoutedCommand MoveDownCommand = new("MoveDownCommand", typeof(ContextMenuSettingControl));


        public ContextMenuSetting ContextMenuSetting
        {
            get { return (ContextMenuSetting)GetValue(ContextMenuSettingProperty); }
            set { SetValue(ContextMenuSettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ContextMenuSetting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContextMenuSettingProperty =
            DependencyProperty.Register("ContextMenuSetting", typeof(ContextMenuSetting), typeof(ContextMenuSettingControl), new PropertyMetadata(null, ContextMenuSettingPropertyChanged));

        private static void ContextMenuSettingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ContextMenuSettingControl)?.Initialize();
        }



        private readonly ContextMenuSettingViewModel _vm;



        public ContextMenuSettingControl()
        {
            InitializeComponent();

            _vm = new ContextMenuSettingViewModel();
            this.Root.DataContext = _vm;

            this.CommandBindings.Add(new CommandBinding(AddCommand, Add_Exec));
            this.CommandBindings.Add(new CommandBinding(RemoveCommand, Remove_Exec, SelectedItem_CanExec));
            this.CommandBindings.Add(new CommandBinding(RenameCommand, Rename_Exec, Rename_CanExec));
            this.CommandBindings.Add(new CommandBinding(MoveUpCommand, MoveUp_Exec, SelectedItem_CanExec));
            this.CommandBindings.Add(new CommandBinding(MoveDownCommand, MoveDown_Exec, SelectedItem_CanExec));

            this.IsVisibleChanged += ContextMenuSettingControl_IsVisibleChanged;
        }



        private void Initialize()
        {
            if (this.ContextMenuSetting != null)
            {
                _vm.Initialize(ContextMenuSetting);
            }
        }

        private void ContextMenuSettingControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                _vm.UpdateSource();
            }
        }

        private void Add_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.SourceComboBox.SelectedItem is not MenuTree element) return;

            if (this.ContextMenuTreeView.SelectedItem is not MenuTree node) return;

            _vm.AddNode(MenuTree.Create(element), node);
        }

        //
        private void SelectedItem_CanExec(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ContextMenuTreeView.SelectedItem is MenuTree node && node.MenuElementType != MenuElementType.None;
        }

        //
        private void Remove_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not MenuTree node) return;

            _vm.RemoveNode(node);
        }

        //
        private void Rename_CanExec(object? sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ContextMenuTreeView.SelectedItem is MenuTree node && node.MenuElementType != MenuElementType.None && node.MenuElementType != MenuElementType.Separator;
        }

        //
        private void Rename_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not MenuTree node) return;

            var param = new RenameWindowParam(node.Label, node.DefaultLabel);

            var dialog = new RenameWindow(param);
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                _vm.RenameNode(node, param.Text);
            }
        }

        //
        private void MoveUp_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not MenuTree node) return;

            _vm.MoveUp(node);
        }

        //
        private void MoveDown_Exec(object? sender, ExecutedRoutedEventArgs e)
        {
            if (this.ContextMenuTreeView.SelectedItem is not MenuTree node) return;

            _vm.MoveDown(node);
        }

        public void Decide()
        {
            _vm.Decide();
        }

        private void ResetButton_Click(object? sender, RoutedEventArgs e)
        {
            _vm.Reset();
        }
    }
}
