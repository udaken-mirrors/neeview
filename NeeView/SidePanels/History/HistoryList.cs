using NeeLaboratory.ComponentModel;
using NeeLaboratory.IO.Search;
using NeeLaboratory.IO.Search.FileNode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class HistoryList : BindableBase
    {
        static HistoryList() => Current = new HistoryList();
        public static HistoryList Current { get; }


        private readonly BookHub _bookHub;

        private string? _filterPath;
        private List<BookHistory> _items = new();
        private bool _isDirty = true;
        private string _searchKeyword = "";
        private BookHistory? _selectedItem;
        private bool _isEnabled;
        private readonly Searcher _searcher;

        private HistoryList()
        {
            var searchContext = new SearchContext()
                .AddProfile(new DateSearchProfile())
                .AddProfile(new SizeSearchProfile())
                .AddProfile(new BookSearchProfile());
            _searcher = new Searcher(searchContext);

            _bookHub = BookHub.Current;

            BookOperation.Current.BookChanged += BookOperation_BookChanged;

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.IsCurrentFolder), (s, e) => UpdateFilterPath());

            BookHistoryCollection.Current.HistoryChanged +=
                (s, e) => AppDispatcher.Invoke(() => BookHistoryCollection_HistoryChanged(s, e));

            BookHub.Current.HistoryListSync +=
                (s, e) => AppDispatcher.Invoke(() => BookHub_HistoryListSync(s, e));

            this.SearchBoxModel = new SearchBoxModel(new HistorySearchBoxComponent(this));

            UpdateFilterPath();
        }


        // 検索ボックスにフォーカスを
        public event EventHandler? SearchBoxFocus;


        public SearchBoxModel SearchBoxModel { get; }

        public bool IsThumbnailVisible
        {
            get
            {
                return Config.Current.History.PanelListItemStyle switch
                {
                    PanelListItemStyle.Content => Config.Current.Panels.ContentItemProfile.ImageWidth > 0.0,
                    PanelListItemStyle.Banner => Config.Current.Panels.BannerItemProfile.ImageWidth > 0.0,
                    _ => false,
                };
            }
        }

        public PanelListItemStyle PanelListItemStyle
        {
            get => Config.Current.History.PanelListItemStyle;
            set => Config.Current.History.PanelListItemStyle = value;
        }

        /// <summary>
        /// パスフィルター
        /// </summary>
        public string? FilterPath
        {
            get { return _filterPath; }
            set
            {
                if (SetProperty(ref _filterPath, value))
                {
                    _isDirty = true;
                    _ = UpdateItemsAsync(CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// 検索キーワード
        /// </summary>
        public string SearchKeyword
        {
            get { return _searchKeyword; }
            set
            {
                if (SetProperty(ref _searchKeyword, value))
                {
                    _isDirty = true;
                    _ = UpdateItemsAsync(CancellationToken.None);
                }
            }
        }

        /// <summary>
        /// 履歴リスト
        /// </summary>
        public List<BookHistory> Items
        {
            get { return _items; }
            private set
            {
                if (SetProperty(ref _items, value))
                {
                    RaisePropertyChanged(nameof(ValidCount));
                }
            }
        }

        /// <summary>
        /// 選択項目
        /// </summary>
        public BookHistory? SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        /// <summary>
        /// 項目数
        /// </summary>
        public int ValidCount => _items.Count;



        private void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
        {
            UpdateFilterPath();
        }


        /// <summary>
        /// 有効フラグ
        /// </summary>
        /// <remarks>
        /// 有効の場合にのみ履歴リストは更新される
        /// </remarks>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (SetProperty(ref _isEnabled, value))
                {
                    if (_isEnabled)
                    {
                        _ = UpdateItemsAsync(CancellationToken.None);
                    }
                }
            }
        }

        /// <summary>
        /// 履歴更新イベント
        /// </summary>
        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            if (e.HistoryChangedType == BookMementoCollectionChangedType.Update) return;
            _isDirty = true;
            _ = UpdateItemsAsync(CancellationToken.None);
        }

        /// <summary>
        /// 履歴同期イベント
        /// </summary>
        private void BookHub_HistoryListSync(object? sender, BookPathEventArgs e)
        {
            if (e.Path is null) return;

            SelectedItem = _items.FirstOrDefault(x => x.Path == e.Path);
        }

        /// <summary>
        /// 履歴フィルター更新
        /// </summary>
        private void UpdateFilterPath()
        {
            FilterPath = Config.Current.History.IsCurrentFolder ? LoosePath.GetDirectoryName(BookOperation.Current.Address) : "";
        }

        /// <summary>
        /// 履歴リスト更新
        /// </summary>
        public async Task UpdateItemsAsync(CancellationToken token)
        {
            if (!_isEnabled) return;

            token.ThrowIfCancellationRequested();
            await Task.Run(() => UpdateItems(false, token));
        }

        /// <summary>
        /// 履歴リスト更新
        /// </summary>
        public void UpdateItems(bool force, CancellationToken token)
        {
            if (!_isEnabled && !force) return;

            if (!_isDirty) return;
            _isDirty = false;

            Items = CreateItems(token);
        }

        /// <summary>
        /// 履歴リスト生成
        /// </summary>
        private List<BookHistory> CreateItems(CancellationToken token)
        {
            List<BookHistory> items;
            lock (BookHistoryCollection.Current.ItemsLock)
            {
                items = BookHistoryCollection.Current.Items
                    .Where(e => string.IsNullOrEmpty(FilterPath) || FilterPath == LoosePath.GetDirectoryName(e.Path))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(_searchKeyword))
            {
                try
                {
                    items = _searcher.Search(_searchKeyword, items, token).Cast<BookHistory>().ToList();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    ToastService.Current.Show(new Toast(ex.Message, "", ToastIcon.Error));
                }
            }

            return items;
        }

        /// <summary>
        /// 最新の履歴リスト取得
        /// </summary>
        public List<BookHistory> GetLatestItems()
        {
            UpdateItems(true, CancellationToken.None);
            return _items;
        }

        /// <summary>
        /// 履歴を戻ることができる？
        /// </summary>
        public bool CanPrevHistory()
        {
            var items = GetLatestItems();

            var index = items.FindIndex(e => e.Path == _bookHub.Address);

            if (index < 0)
            {
                return items.Any();
            }
            else
            {
                return index < items.Count - 1;
            }
        }

        /// <summary>
        /// 履歴を戻る
        /// </summary>
        public void PrevHistory()
        {
            var items = GetLatestItems();

            if (_bookHub.IsLoading || items.Count <= 0) return;

            var index = items.FindIndex(e => e.Path == _bookHub.Address);

            var prev = index < 0
                ? items.First()
                : index < items.Count - 1 ? items[index + 1] : null;

            if (prev != null)
            {
                _bookHub.RequestLoad(this, prev.Path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.TextResources.GetString("Notice.HistoryTerminal"));
            }
        }

        /// <summary>
        /// 履歴を進めることができる？
        /// </summary>
        public bool CanNextHistory()
        {
            var items = GetLatestItems();

            var index = items.FindIndex(e => e.Path == _bookHub.Address);
            return index > 0;
        }

        /// <summary>
        /// 履歴を進める
        /// </summary>
        public void NextHistory()
        {
            var items = GetLatestItems();

            var index = items.FindIndex(e => e.Path == _bookHub.Address);
            if (index > 0)
            {
                var next = items[index - 1];
                _bookHub.RequestLoad(this, next.Path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SelectHistoryMaybe | BookLoadOption.IsBook, true);
            }
            else
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.TextResources.GetString("Notice.HistoryLatest"));
            }
        }

        /// <summary>
        /// 履歴削除
        /// </summary>
        /// <param name="items">削除項目</param>
        public void Remove(IEnumerable<BookHistory> items)
        {
            if (items == null) return;

            // 位置ずらし
            SelectedItem = GetNeighbor(_selectedItem, items);

            // 削除実行
            BookHistoryCollection.Current.Remove(items.Select(e => e.Path));
        }

        /// <summary>
        /// となりを取得
        /// </summary>
        /// <param name="item">基準項目</param>
        /// <param name="excludes">除外項目</param>
        /// <returns></returns>
        private BookHistory? GetNeighbor(BookHistory? item, IEnumerable<BookHistory> excludes)
        {
            var items = _items;

            if (items == null || items.Count <= 0) return null;

            if (item is null) return items[0];

            int index = items.IndexOf(item);
            if (index < 0) return items[0];

            var next = items
                .Skip(index)
                .Concat(items.Take(index))
                .Except(excludes)
                .FirstOrDefault();

            return next;
        }


        public SearchKeywordAnalyzeResult SearchKeywordAnalyze(string keyword)
        {
            try
            {
                return new SearchKeywordAnalyzeResult(_searcher.Analyze(keyword));
            }
            catch (Exception ex)
            {
                return new SearchKeywordAnalyzeResult(ex);
            }
        }

        /// <summary>
        /// 検索ボックスにフォーカス要求
        /// </summary>
        public void RaiseSearchBoxFocus()
        {
            SearchBoxFocus?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// 検索ボックスコンポーネント
        /// </summary>
        public class HistorySearchBoxComponent : ISearchBoxComponent
        {
            private readonly HistoryList _self;

            public HistorySearchBoxComponent(HistoryList self)
            {
                _self = self;
            }

            public HistoryStringCollection? History => BookHistoryCollection.Current.BookHistorySearchHistory;

            public bool IsIncrementalSearchEnabled => Config.Current.System.IsIncrementalSearchEnabled;

            public SearchKeywordAnalyzeResult Analyze(string keyword) => _self.SearchKeywordAnalyze(keyword);

            public void Search(string keyword) => _self.SearchKeyword = keyword;
        }
    }


}
