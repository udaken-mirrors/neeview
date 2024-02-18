using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
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
using System.Windows.Shapes;

namespace NeeView.Setting
{
    /// <summary>
    /// CommandResetWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandResetWindow : Window
    {
        private readonly CommandResetWindowViewModel _vm;

        /// <summary>
        /// constructor
        /// </summary>
        public CommandResetWindow()
        {
            InitializeComponent();

            _vm = new CommandResetWindowViewModel();
            this.DataContext = _vm;

            this.Loaded += CommandResetWindow_Loaded;
            this.KeyDown += CommandResetWindow_KeyDown;
        }

        private void CommandResetWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void CommandResetWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.OkButton.Focus();
        }

        /// <summary>
        /// 現在の設定でコマンドテーブルを生成
        /// </summary>
        /// <returns></returns>
        public CommandCollection CreateCommandMemento()
        {
            return CommandTable.CreateDefaultMemento(_vm.InputScheme);
        }
    }

    /// <summary>
    /// CommandResetWindow ViewModel
    /// </summary>
    public class CommandResetWindowViewModel : BindableBase
    {
        /// <summary>
        /// InputScheme 表示テーブル
        /// </summary>
        public Dictionary<InputScheme, string> InputSchemeList { get; } = new Dictionary<InputScheme, string>
        {
            [InputScheme.TypeA] = Properties.TextResources.GetString("InputScheme.TypeA"),
            [InputScheme.TypeB] = Properties.TextResources.GetString("InputScheme.TypeB"),
            [InputScheme.TypeC] = Properties.TextResources.GetString("InputScheme.TypeC")
        };

        /// <summary>
        /// InputScheme 説明テーブル
        /// </summary>
        public Dictionary<InputScheme, string> InputSchemeNoteList { get; } = new Dictionary<InputScheme, string>
        {
            [InputScheme.TypeA] = ResourceService.Replace(Properties.TextResources.GetString("InputScheme.TypeA.Remarks")),
            [InputScheme.TypeB] = ResourceService.Replace(Properties.TextResources.GetString("InputScheme.TypeB.Remarks")),
            [InputScheme.TypeC] = ResourceService.Replace(Properties.TextResources.GetString("InputScheme.TypeC.Remarks")),
        };

        /// <summary>
        /// InputScheme property.
        /// </summary>
        private InputScheme _InputScheme;
        public InputScheme InputScheme
        {
            get { return _InputScheme; }
            set { if (_InputScheme != value) { _InputScheme = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(InputSchemeNote)); } }
        }

        /// <summary>
        /// InputSchemeNote property.
        /// </summary>
        public string InputSchemeNote => InputSchemeNoteList[InputScheme];

        /// <summary>
        /// OkCommand command.
        /// </summary>
        private RelayCommand<Window>? _OkCommand;
        public RelayCommand<Window> OkCommand
        {
            get { return _OkCommand = _OkCommand ?? new RelayCommand<Window>(OkCommand_Executed); }
        }

        private void OkCommand_Executed(Window? window)
        {
            if (window is null) return;

            window.DialogResult = true;
            window.Close();
        }

        /// <summary>
        /// CancelCommand command.
        /// </summary>
        private RelayCommand<Window>? _CancelCommand;
        public RelayCommand<Window> CancelCommand
        {
            get { return _CancelCommand = _CancelCommand ?? new RelayCommand<Window>(CancelCommand_Executed); }
        }

        private void CancelCommand_Executed(Window? window)
        {
            if (window is null) return;

            window.DialogResult = false;
            window.Close();
        }
    }
}
