using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using NeeView.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// HistoryListView.xaml の相互作用ロジック
    /// </summary>
    public partial class PlaylistView : UserControl
    {
        private PlaylistViewModel _vm;


        public PlaylistView(PlaylistHub model)
        {
            InitializeComponent();
            InitializeCommand();

            _vm = new PlaylistViewModel(model);
            this.DockPanel.DataContext = _vm;

            _vm.RenameRequest +=
                (s, e) => Rename();

            this.PlaylistComboBox.DropDownOpened +=
                (s, e) => _vm.UpdatePlaylistCollection();
       }


        public readonly static RoutedCommand RenameCommand = new RoutedCommand(nameof(RenameCommand), typeof(PlaylistView), new InputGestureCollection() { new KeyGesture(Key.F2) });


        private void InitializeCommand()
        {
            this.PlaylistComboBox.CommandBindings.Add(new CommandBinding(RenameCommand, RenameCommand_Execute, RenameCommand_CanExecute));
        }

        private void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _vm.RenameCommand.CanExecute(e.Parameter);
        }

        private void RenameCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            _vm.RenameCommand.Execute(e.Parameter);
        }


        private void Rename()
        {
            var comboBox = this.PlaylistComboBox;
            comboBox.UpdateLayout();

            var textBlock = VisualTreeUtility.FindVisualChild<TextBlock>(comboBox, "NameTextBlock");
            if (textBlock is null) return;

            var rename = new RenameControl(textBlock) { StoredFocusTarget = comboBox };
            rename.IsInvalidFileNameChars = true;

            rename.Closed += (s, e) =>
            {
                if (e.IsChanged)
                {
                    _vm.Rename(e.NewValue);
                }
            };

            rename.Open();
        }
    }
}
