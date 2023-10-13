using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Windows.Data;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// AddressBar : ViewModel
    /// </summary>
    public class AddressBarViewModel : BindableBase
    {
        private AddressBar _model;
        private RelayCommand<KeyValuePair<int, QueryPath>>? _moveToHistory;
        private RelayCommand? _togglePageModeCommand;
        private readonly DelayValue<bool> _isLoading;


        public AddressBarViewModel(AddressBar model)
        {
            _model = model;

            _isLoading = new DelayValue<bool>();
            _isLoading.ValueChanged += (s, e) => RaisePropertyChanged(nameof(IsLoading));
            PageFrameBoxPresenter.Current.Loading += Presenter_Loading;
        }


        public AddressBar Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public Config Config => Config.Current;

        public bool IsLoading => _isLoading.Value;

        public Dictionary<string, RoutedUICommand> BookCommands
        {
            get { return RoutedCommandTable.Current.Commands; }
        }

        public BookSettingConfig BookSetting
        {
            get { return Config.Current.BookSetting; }
        }


        public List<KeyValuePair<int, QueryPath>> GetHistory(int direction, int size)
        {
            return BookHubHistory.Current.GetHistory(direction, size);
        }


        public RelayCommand<KeyValuePair<int, QueryPath>> MoveToHistory
        {
            get { return _moveToHistory = _moveToHistory ?? new RelayCommand<KeyValuePair<int, QueryPath>>(MoveToHistory_Executed); }
        }

        public RelayCommand TogglePageModeCommand
        {
            get { return _togglePageModeCommand = _togglePageModeCommand ?? new RelayCommand(TogglePageModeCommand_Execute); }
        }



        private void Presenter_Loading(object? sender, BookPathEventArgs e)
        {
            if (e.Path != null)
            {
                _isLoading.SetValue(true, 1000);
            }
            else
            {
                _isLoading.SetValue(false, 0);
            }
        }

        private void MoveToHistory_Executed(KeyValuePair<int, QueryPath> item)
        {
            BookHubHistory.Current.MoveToHistory(item);
        }

        private void TogglePageModeCommand_Execute()
        {
            BookSettings.Current.TogglePageMode(+1, true);
        }

    }
}
