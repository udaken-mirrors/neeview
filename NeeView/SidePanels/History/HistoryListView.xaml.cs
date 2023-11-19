using System;
using System.Collections.Generic;
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
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// HistoryListView.xaml の相互作用ロジック
    /// </summary>
    public partial class HistoryListView : UserControl
    {
        private readonly HistoryListViewModel _vm;


        public HistoryListView(HistoryList model)
        {
            InitializeComponent();

            _vm = new HistoryListViewModel(model);
            this.DockPanel.DataContext = _vm;

            model.SearchBoxFocus += HistoryList_SearchBoxFocus;
        }


        /// <summary>
        /// 検索ボックスのフォーカス要求処理
        /// </summary>
        private void HistoryList_SearchBoxFocus(object? sender, EventArgs e)
        {
            this.SearchBox.FocusAsync();
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
