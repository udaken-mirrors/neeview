namespace NeeView
{
    public class HistoryListPresenter
    {
        private readonly HistoryListView _historyListView;
        private readonly HistoryList _historyList;
        private readonly HistoryListBoxViewModel _historyListBoxViewModel;
        private HistoryListBox? _historyListBox;


        public HistoryListPresenter(HistoryListView historyListView, HistoryList historyList)
        {
            _historyListView = historyListView;
            _historyList = historyList;

            _historyListBoxViewModel = new HistoryListBoxViewModel(historyList);
            UpdateListBoxContent();

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.PanelListItemStyle), (s, e) => UpdateListBoxContent());
        }


        public HistoryListView HistoryListView => _historyListView;
        public HistoryListBox? HistoryListBox => _historyListBox;
        public HistoryList HistoryList => _historyList;


        private void UpdateListBoxContent()
        {
            _historyListBox = new HistoryListBox(_historyListBoxViewModel);
            _historyListView.ListBoxContent.Content = _historyListBox;
        }


        public void Refresh()
        {
            _historyListBox?.Refresh();
        }

        public void FocusAtOnce()
        {
            _historyListBox?.FocusAtOnce();
        }
    }
}
