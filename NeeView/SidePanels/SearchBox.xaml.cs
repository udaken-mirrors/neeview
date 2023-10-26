using NeeView.Threading;
using NeeView.Windows.Media;
using System;
using System.Collections;
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

namespace NeeView
{
    /// <summary>
    /// SearchBox.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchBox : UserControl
    {
        private readonly DelayAction _delayAction;

        public SearchBox()
        {
            InitializeComponent();
            this.SearchBoxRoot.DataContext = this;

            _delayAction = new DelayAction();
        }

        /// <summary>
        /// テキストボックスのキーボードフォーカス変更イベント
        /// </summary>
        public event EventHandler<FocusChangedEventArgs>? SearchBoxFocusChanged;


        /// <summary>
        /// 検索エラーメッセージ
        /// </summary>
        public string SearchKeywordErrorMessage
        {
            get { return (string)GetValue(SearchKeywordErrorMessageProperty); }
            set { SetValue(SearchKeywordErrorMessageProperty, value); }
        }

        public static readonly DependencyProperty SearchKeywordErrorMessageProperty =
            DependencyProperty.Register("SearchKeywordErrorMessage", typeof(string), typeof(SearchBox), new PropertyMetadata(""));

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SearchBox), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 検索キーワード候補。検索履歴とか。
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SearchBox), new PropertyMetadata(null));

        /// <summary>
        /// 検索コマンド
        /// </summary>
        public ICommand? SearchCommand
        {
            get { return (ICommand)GetValue(SearchCommandProperty); }
            set { SetValue(SearchCommandProperty, value); }
        }

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(SearchBox), new PropertyMetadata(null));

        /// <summary>
        /// クリアボタン
        /// </summary>
        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Text = "";
        }

        /// <summary>
        /// 単キーのショートカット無効
        /// </summary>
        private void Control_KeyDown_IgnoreSingleKeyGesture(object? sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;
        }

        /// <summary>
        /// キー入力
        /// </summary>
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            KeyExGesture.AllowSingleKey = false;

            // 検索実行
            if (e.Key == Key.Enter)
            {
                Text = this.SearchBoxComboBox.Text;
                Search();
            }
        }

        /// <summary>
        /// テキストボックスのキーボードフォーカス変更イベント
        /// </summary>
        private void SearchBox_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //Debug.WriteLine($"SBF.K: {this.SearchBox.IsKeyboardFocusWithin}");
            SearchBoxFocusChanged?.Invoke(this, new FocusChangedEventArgs(this.SearchBoxComboBox.IsKeyboardFocusWithin));
        }

        /// <summary>
        /// テキストボックスのテキスト 遅延反映
        /// </summary>
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.OriginalSource is not TextBox textBox) return;

            _delayAction.Request(() =>
            {
                this.Text = textBox.Text;
            },
            TimeSpan.FromMilliseconds(500));
        }

        /// <summary>
        /// テキストボックスにフォーカス
        /// </summary>
        public bool FocusEditableTextBox()
        {
            return this.SearchBoxComboBox.Focus();
        }

        /// <summary>
        /// 検索実行
        /// </summary>
        private void Search()
        {
            if (SearchCommand?.CanExecute(null) == true)
            {
                SearchCommand?.Execute(null);
            }
        }
    }
}
