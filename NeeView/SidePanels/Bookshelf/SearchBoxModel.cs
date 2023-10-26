using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;
using NeeLaboratory.Windows.Input;
using System;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class SearchBoxModel : INotifyPropertyChanged
    {
        private static readonly SearchKeyAnalyzer _searchKeyAnalyzer = new();

        private string? _keyword;
        private string? _keywordErrorMessage;
        private RelayCommand? _searchCommand;

        private readonly ISearchBoxComponent _component;

        public SearchBoxModel(ISearchBoxComponent component)
        {
            _component = component;
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string? Keyword
        {
            get { return _keyword; }
            set
            {
                if (SetProperty(ref _keyword, value))
                {
                    UpdateSearchKeywordErrorMessage();
                    IncrementalSearch();
                    RaisePropertyChanged(nameof(FixedKeyword));
                }
            }
        }

        /// <summary>
        /// 検索キーワードの正規化
        /// </summary>
        public string FixedKeyword => _keyword?.Trim() ?? "";

        /// <summary>
        /// 検索キーワードエラーメッセージ
        /// </summary>
        public string? KeywordErrorMessage
        {
            get => _keywordErrorMessage;
            set => SetProperty(ref _keywordErrorMessage, value);
        }

        /// <summary>
        /// 検索キーワード履歴
        /// </summary>
        public HistoryStringCollection? History => _component.History;

        /// <summary>
        /// インクリメンタルサーチフラグ
        /// </summary>
        public bool IsIncrementalSearchEnabled => _component.IsIncrementalSearchEnabled;

        /// <summary>
        /// 検索コマンド
        /// </summary>
        public RelayCommand SearchCommand
        {
            get { return _searchCommand = _searchCommand ?? new RelayCommand(Search); }
        }



        /// <summary>
        /// 検索実行
        /// </summary>
        public void Search()
        {
            // 検索を重複させないための処置
            if (!IsIncrementalSearchEnabled)
            {
                _component.Search(FixedKeyword);
            }

            // 確定検索なので履歴更新
            UpdateSearchHistory();
        }

        /// <summary>
        /// 逐次検索
        /// </summary>
        private void IncrementalSearch()
        {
            //Debug.WriteLine($"Search: {_searchKeyword.Value}");
            // インクリメンタルサーチなら検索実行
            if (IsIncrementalSearchEnabled)
            {
                _component.Search(FixedKeyword);
            }
        }

        /// <summary>
        /// 検索履歴更新
        /// </summary>
        public void UpdateSearchHistory()
        {
            var keyword = FixedKeyword;
            if (string.IsNullOrEmpty(keyword)) return;
            History?.Append(keyword);
        }

        /// <summary>
        /// 検索キーワードのフォーマットチェック
        /// </summary>
        public void UpdateSearchKeywordErrorMessage()
        {
            var keyword = FixedKeyword;

            try
            {
                _searchKeyAnalyzer.Analyze(keyword);
                KeywordErrorMessage = null;
            }
            catch (SearchKeywordOptionException ex)
            {
                KeywordErrorMessage = string.Format(Properties.Resources.Notice_SearchKeywordOptionError, ex.Option);
            }
            catch (SearchKeywordDateTimeException)
            {
                KeywordErrorMessage = Properties.Resources.Notice_SearchKeywordDateTimeError;
            }
            catch (SearchKeywordRegularExpressionException ex)
            {
                KeywordErrorMessage = ex.InnerException?.Message;
            }
            catch (Exception ex)
            {
                KeywordErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// 環境変更による検索キーワードリセット
        /// </summary>
        /// <param name="keyword"></param>
        public void ResetInputKeyword(string? keyword)
        {
            if (keyword != FixedKeyword)
            {
                UpdateSearchHistory();
                // 入力文字のみ更新
                _keyword = keyword;
                RaisePropertyChanged(nameof(Keyword));
            }
        }
    }


}
