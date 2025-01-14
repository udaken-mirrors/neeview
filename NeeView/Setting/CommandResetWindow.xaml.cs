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


        public CommandResetWindow()
        {
            InitializeComponent();

            _vm = new CommandResetWindowViewModel();
            this.DataContext = _vm;

            this.Loaded += CommandResetWindow_Loaded;
            this.KeyDown += CommandResetWindow_KeyDown;
        }


        public InputScheme InputScheme => _vm.InputScheme;


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
    }


    public class CommandResetWindowViewModel : BindableBase
    {
        private InputScheme _inputScheme;
        private RelayCommand<Window>? _okCommand;
        private RelayCommand<Window>? _cancelCommand;

        public InputScheme InputScheme
        {
            get { return _inputScheme; }
            set { SetProperty(ref _inputScheme, value); }
        }

        public RelayCommand<Window> OkCommand
        {
            get { return _okCommand = _okCommand ?? new RelayCommand<Window>(OkCommand_Executed); }
        }

        public RelayCommand<Window> CancelCommand
        {
            get { return _cancelCommand = _cancelCommand ?? new RelayCommand<Window>(CancelCommand_Executed); }
        }


        private void OkCommand_Executed(Window? window)
        {
            if (window is null) return;

            window.DialogResult = true;
            window.Close();
        }

        private void CancelCommand_Executed(Window? window)
        {
            if (window is null) return;

            window.DialogResult = false;
            window.Close();
        }
    }
}
