using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Diagnostics;
using NeeView.IO;
using NeeLaboratory.Threading.Tasks;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeeLaboratory.Threading.Jobs;
using NeeView.Interop;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    /// <summary>
    /// 本の管理
    /// ロード、本の操作はここを通す
    /// </summary>
    public sealed class BookHub : BindableBase, IDisposable
    {
        // Singleton
        static BookHub() => Current = new BookHub();
        public static BookHub Current { get; }

        #region NormalizePathName

        // パス名の正規化
        private static string GetNormalizePathName(string source)
        {
            // 区切り文字修正
            source = new System.Text.RegularExpressions.Regex(@"[/\\]").Replace(source, "\\").TrimEnd('\\');

            // ドライブレター修正
            source = new System.Text.RegularExpressions.Regex(@"^[a-z]:").Replace(source, m => m.Value.ToUpper());
            source = new System.Text.RegularExpressions.Regex(@":$").Replace(source, ":\\");

            var longPath = new StringBuilder(1024);
            if (0 == NativeMethods.GetLongPathName(source, longPath, longPath.Capacity))
            {
                return source;
            }

            string dist = longPath.ToString();
            return longPath.ToString();
        }

        #endregion

        private Toast? _bookHubToast;
        private bool _isLoading;
        private Book? _book;
        private string? _address;
        private readonly BookHubCommandEngine _commandEngine;
        private bool _historyEntry;
        private bool _historyRemoved;
        private int _requestLoadCount;
        private readonly DisposableCollection _disposables;

        private BookHub()
        {
            _disposables = new DisposableCollection();

            _disposables.Add(SubscribeBookChanged(
                (s, e) =>
                {
                    if (_disposedValue) return;

                    var book = _book;
                    if (book?.NotFoundStartPage != null && book.Pages.Count > 0)
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.BookName, string.Format(Properties.Resources.Notice_CannotOpen, LoosePath.GetFileName(book.NotFoundStartPage)), null, 2.0);
                    }
                    else
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.BookName, LoosePath.GetFileName(Address), null, 2.0, e.BookMementoType);
                    }
                }));

            _disposables.Add(BookHistoryCollection.Current.SubscribeHistoryChanged(
                BookHistoryCollection_HistoryChanged));

            _disposables.Add(BookmarkCollection.Current.SubscribeBookmarkChanged(
                (s, e) => BookmarkChanged?.Invoke(s, e)));

            _disposables.Add(BookSettingPresenter.Current.SubscribeSettingChanged(
                (s, e) => _book?.Restore(BookSettingPresenter.Current.LatestSetting.ToBookMemento())));

            // command engine
            _commandEngine = new BookHubCommandEngine();
            _disposables.Add(_commandEngine.SubscribeIsBusyChanged(
                (s, e) => IsBusyChanged?.Invoke(s, e)));
            _commandEngine.StartEngine();
            _disposables.Add(_commandEngine);

            // アプリ終了前の開放予約
            ApplicationDisposer.Current.Add(this);
        }


        // 本の変更開始通知
        public event EventHandler<BookChangingEventArgs>? BookChanging;

        public IDisposable SubscribeBookChanging(EventHandler<BookChangingEventArgs> handler)
        {
            BookChanging += handler;
            return new AnonymousDisposable(() => BookChanging -= handler);
        }

        // 本の変更通知
        public event EventHandler<BookChangedEventArgs>? BookChanged;

        public IDisposable SubscribeBookChanged(EventHandler<BookChangedEventArgs> handler)
        {
            BookChanged += handler;
            return new AnonymousDisposable(() => BookChanged -= handler);
        }

        // 新しいロードリクエスト
        public event EventHandler<BookHubPathEventArgs>? LoadRequested;

        public IDisposable SubscribeLoadRequested(EventHandler<BookHubPathEventArgs> handler)
        {
            LoadRequested += handler;
            return new AnonymousDisposable(() => LoadRequested -= handler);
        }

        // ロード中通知
        public event EventHandler<BookHubPathEventArgs>? Loading;

        public IDisposable SubscribeLoading(EventHandler<BookHubPathEventArgs> handler)
        {
            Loading += handler;
            return new AnonymousDisposable(() => Loading -= handler);
        }

        // コマンドエンジン処理中通知
        public event EventHandler<JobIsBusyChangedEventArgs>? IsBusyChanged;

        public IDisposable SubscribeIsBusyChanged(EventHandler<JobIsBusyChangedEventArgs> handler)
        {
            IsBusyChanged += handler;
            return new AnonymousDisposable(() => IsBusyChanged -= handler);
        }

        // ViewContentsの変更通知
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? ViewContentsChanged;

        public IDisposable SubscribeViewContentsChanged(EventHandler<ViewContentSourceCollectionChangedEventArgs> handler)
        {
            ViewContentsChanged += handler;
            return new AnonymousDisposable(() => ViewContentsChanged -= handler);
        }

        // NextContentsの変更通知
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? NextContentsChanged;

        public IDisposable SubscribeNextContentsChanged(EventHandler<ViewContentSourceCollectionChangedEventArgs> handler)
        {
            NextContentsChanged += handler;
            return new AnonymousDisposable(() => NextContentsChanged -= handler);
        }

        // 空ページメッセージ
        public event EventHandler<BookHubMessageEventArgs>? EmptyMessage;

        public IDisposable SubscribeEmptyMessage(EventHandler<BookHubMessageEventArgs> handler)
        {
            EmptyMessage += handler;
            return new AnonymousDisposable(() => EmptyMessage -= handler);
        }

        // 空ページメッセージ その２
        public event EventHandler<BookHubMessageEventArgs>? EmptyPageMessage;

        public IDisposable SubscribeEmptyPageMessage(EventHandler<BookHubMessageEventArgs> handler)
        {
            EmptyPageMessage += handler;
            return new AnonymousDisposable(() => EmptyPageMessage -= handler);
        }

        // フォルダー列更新要求
        public event EventHandler<FolderListSyncEventArgs>? FolderListSync;

        public IDisposable SubscribeFolderListSync(EventHandler<FolderListSyncEventArgs> handler)
        {
            FolderListSync += handler;
            return new AnonymousDisposable(() => FolderListSync -= handler);
        }

        // 履歴リスト更新要求
        public event EventHandler<BookHubPathEventArgs>? HistoryListSync;

        public IDisposable SubscribeHistoryListSync(EventHandler<BookHubPathEventArgs> handler)
        {
            HistoryListSync += handler;
            return new AnonymousDisposable(() => HistoryListSync -= handler);
        }

        // 履歴に追加、削除された
        public event EventHandler<BookMementoCollectionChangedArgs>? HistoryChanged;

        public IDisposable SubscribeHistoryChanged(EventHandler<BookMementoCollectionChangedArgs> handler)
        {
            HistoryChanged += handler;
            return new AnonymousDisposable(() => HistoryChanged -= handler);
        }

        // ブックマークにに追加、削除された
        public event EventHandler<BookmarkCollectionChangedEventArgs>? BookmarkChanged;

        public IDisposable SubscribeBookmarkChanged(EventHandler<BookmarkCollectionChangedEventArgs> handler)
        {
            BookmarkChanged += handler;
            return new AnonymousDisposable(() => BookmarkChanged -= handler);
        }

        // アドレスが変更された
        public event EventHandler? AddressChanged;

        public IDisposable SubscribeAddressChanged(EventHandler handler)
        {
            AddressChanged += handler;
            return new AnonymousDisposable(() => AddressChanged -= handler);
        }



        // アドレス
        public string? Address
        {
            get { return _address; }
            private set
            {
                if (_disposedValue) return;

                if (_address != value)
                {
                    _address = value;
                    Config.Current.StartUp.LastBookPath = Address;
                    AddressChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// ロード可能フラグ
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// ロード中フラグ
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            private set
            {
                if (_disposedValue) return;

                if (SetProperty(ref _isLoading, value))
                {
                    BookSettingPresenter.Current.IsLocked = _isLoading;
                }
            }
        }

        /// <summary>
        /// ロード中ブックのパス
        /// </summary>
        public string? LoadingPath { get; private set; }

        public bool IsBusy => _commandEngine.IsBusy;

        /// <summary>
        /// ロードリクエストカウント.
        /// 名前変更時の再読込判定に使用される
        /// </summary>
        public int RequestLoadCount => _requestLoadCount;



        /// <summary>
        /// 現在のブック
        /// </summary>
        public Book? GetCurrentBook() => _book;

        #region Callback Methods

        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            if (_disposedValue) return;

            HistoryChanged?.Invoke(sender, e);

            var book = _book;
            if (book is null) return;

            // 履歴削除されたものを履歴登録しないようにする
            if (e.HistoryChangedType == BookMementoCollectionChangedType.Remove && (book.Path == e.Key || e.Key == null))
            {
                _historyRemoved = true;
            }
        }

        private void BookViewer_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            var book = _book;
            if (book is null) return;

            _historyRemoved = false;

            bool allowUpdateHistory = !book.IsKeepHistoryOrder || Config.Current.History.IsForceUpdateHistory;

            // 履歴更新
            if (allowUpdateHistory && !_historyEntry && CanHistory(book))
            {
                _historyEntry = true;
                var memento = book.CreateMemento();
                if (memento is not null)
                {
                    BookHistoryCollection.Current.Add(memento, false);
                }
            }

            var viewPages = e.ViewPageCollection?.Collection.Where(x => x != null).Select(x => x.Page).ToList() ?? new List<Page>();
            book.Pages.SetViewPageFlag(viewPages);

            ViewContentsChanged?.Invoke(sender, e);
        }

        private void BookViewer_NextContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (_book is null) return;

            NextContentsChanged?.Invoke(sender, e);
        }

        private void BookSource_DartyBook(object? sender, EventArgs e)
        {
            RequestLoad(this, this.Address, null, BookLoadOption.ReLoad | BookLoadOption.IsBook, false);
        }

        #endregion Callback Methods

        #region Requests

        /// <summary>
        /// リクエスト：フォルダーを開く
        /// </summary>
        /// <param name="path">開く場所</param>
        /// <param name="start">ページの指定</param>
        /// <param name="option"></param>
        /// <param name="isRefreshFolderList">フォルダーリストを同期する？</param>
        /// <returns></returns>
        public BookHubCommandLoad? RequestLoad(object? sender, string? path, string? start, BookLoadOption option, bool isRefreshFolderList)
        {
            if (path == null) return null;

            if (_disposedValue) return null;

            if (!this.IsEnabled) return null;

            path = LoosePath.NormalizeSeparator(path);
            var sourcePath = path;
            if (FileShortcut.IsShortcut(path) && (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
            {
                var shortcut = new FileShortcut(path);
                if (shortcut.TryGetTargetPath(out var target))
                {
                    path = target;
                }
            }

            if (path.StartsWith("pagemark:"))
            {
                path = Config.Current.Playlist.PagemarkPlaylist;
            }

            path = GetNormalizePathName(path);

            ////DebugTimer.Start($"\nStart: {path}");

            LoadRequested?.Invoke(this, new BookHubPathEventArgs(path));

            if (_book?.Path == path && option.HasFlag(BookLoadOption.SkipSamePlace)) return null;

            this.Address = path;

            Interlocked.Increment(ref _requestLoadCount);

            var command = new BookHubCommandLoad(this, new BookHubCommandLoadArgs(path, sourcePath)
            {
                Sender = sender,
                //Path = path,
                //SourcePath = sourcePath,
                StartEntry = start,
                Option = option,
                IsRefreshFolderList = isRefreshFolderList
            });

            _commandEngine.Enqueue(command);

            return command;
        }


        // アンロード可能?
        public bool CanUnload()
        {
            if (_disposedValue) return false;

            return _book != null || _commandEngine.IsBusy;
        }

        /// <summary>
        /// リクエスト：フォルダーを閉じる
        /// </summary>
        /// <param name="isClearViewContent"></param>
        /// <returns></returns>
        public BookHubCommandUnload RequestUnload(object? sender, bool isClearViewContent, string? message = null)
        {
            ThrowIfDisposed();

            var command = new BookHubCommandUnload(this, new BookHubCommandUnloadArgs()
            {
                Sender = sender,
                IsClearViewContent = isClearViewContent,
                Message = message
            });

            _commandEngine.Enqueue(command);

            return command;
        }


        // 再読込可能？
        public bool CanReload()
        {
            if (_disposedValue) return false;

            return (!string.IsNullOrWhiteSpace(Address));
        }

        /// <summary>
        /// リクエスト：再読込
        /// </summary>
        public void RequestReLoad(object sender)
        {
            RequestReLoad(sender, null);
        }

        /// <summary>
        /// リクエスト：再読込
        /// </summary>
        public void RequestReLoad(object sender, string? start)
        {
            if (_disposedValue) return;

            if (_isLoading || Address == null) return;

            var book = _book;
            BookLoadOption options = book != null ? (book.LoadOption & BookLoadOption.KeepHistoryOrder) | BookLoadOption.Resume : BookLoadOption.None;
            RequestLoad(sender, Address, start, options | BookLoadOption.IsBook | BookLoadOption.IgnoreCache, true);
        }

        // 上の階層に移動可能？
        public bool CanLoadParent()
        {
            if (_disposedValue) return false;

            var parent = _book?.BookAddress?.Place;
            return parent?.Path != null && parent.Scheme == QueryScheme.File;
        }

        /// <summary>
        /// リクエスト：上の階層に移動
        /// </summary>
        public void RequestLoadParent(object sender)
        {
            if (_disposedValue) return;

            var bookAddress = _book?.BookAddress;

            var current = bookAddress?.TargetPath;
            if (current is null) return;

            var parent = bookAddress?.Place;
            if (parent is null) return;

            if (parent.Path != null && parent.Scheme == QueryScheme.File)
            {
                var entryName = current.SimplePath[parent.SimplePath.Length..].TrimStart(LoosePath.Separators);
                var option = BookLoadOption.SkipSamePlace;
                RequestLoad(sender, parent.SimplePath, entryName, option, true);
            }
        }

        #endregion Requests

        #region BookHubCommand.Load

        /// <summary>
        /// ロード中状態更新
        /// </summary>
        /// <param name="path"></param>
        private void NotifyLoading(string? path)
        {
            if (_disposedValue) return;

            this.LoadingPath = path;
            this.IsLoading = (path != null);
            Loading?.Invoke(this, new BookHubPathEventArgs(path));
        }

        /// <summary>
        /// 本を読み込む
        /// </summary>
        public async Task LoadAsync(BookHubCommandLoadArgs args, CancellationToken token)
        {
            if (_disposedValue) return;

            token.ThrowIfCancellationRequested();

            ////DebugTimer.Check("LoadAsync...");

            // 現在の設定を記憶
            var oldBook = _book;
            var lastBookMemento = oldBook?.Path != null ? oldBook.CreateMemento() : null;

            Address = args.SourcePath;

            // 本の変更開始通知
            // NOTE: パスはまだページの可能性があるので不完全
            BookChanging?.Invoke(this, new BookChangingEventArgs(Address));

            // 現在の本を開放
            UnloadCore();

            string place = args.Path;

            if (_bookHubToast != null)
            {
                _bookHubToast.Cancel();
                _bookHubToast = null;
            }

            try
            {
                // address
                var address = await BookAddress.CreateAsync(new QueryPath(args.Path), new QueryPath(args.SourcePath), args.StartEntry, Config.Current.System.ArchiveRecursiveMode, args.Option, token);

                // Now Loading ON
                NotifyLoading(args.Path);

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    HistoryListSync?.Invoke(this, new BookHubPathEventArgs(address.TargetPath.SimplePath));
                }

                // 本の設定
                var setting = CreateOpenBookMemento(address.TargetPath.SimplePath, lastBookMemento, args.Option);

                // 最終ページなら初期化？
                bool isResetLastPage = address.EntryName == null && Config.Current.BookSettingPolicy.Page == BookSettingPageSelectMode.RestoreOrDefaultReset;

                address.EntryName = address.EntryName ?? LoosePath.NormalizeSeparator(setting.Page);
                place = address.SystemPath;

                // 移動履歴登録
                BookHubHistory.Current.Add(args.Sender, address.TargetPath);

                // フォルダーリスト更新
                if (args.IsRefreshFolderList)
                {
                    FolderListSync?.Invoke(this, new FolderListSyncEventArgs(address.TargetPath.SimplePath, address.Place.SimplePath, false));
                }

                var isNew = BookMementoCollection.Current.GetValid(address.TargetPath.SimplePath) == null;

                // Load本体
                var loadOption = args.Option | (isResetLastPage ? BookLoadOption.ResetLastPage : BookLoadOption.None);
                var book = await LoadAsyncCore(args.Sender, address, loadOption, setting, isNew, token);

                _historyEntry = false;
                _historyRemoved = false;

                var bookSetting = BookSettingConfigExtensions.FromBookMement(book.CreateMemento());
                if (bookSetting is not null)
                {
                    // 本の設定を更新
                    BookSettingPresenter.Current.SetLatestSetting(bookSetting);
                }

                this.Address = book.Path;
                _book = book;
                SubscribeBook(book);

                await WaitBookReadyAsync(book, token);

                ////DebugTimer.Check("LoadCore");

                BookChanged?.Invoke(this, new BookChangedEventArgs(book.Path, book, GetBookMementoType(book)));

                // ページがなかった時の処理
                if (book != null && book.Pages.Count <= 0)
                {
                    ResetBookMementoPage(book.Path);

                    EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(string.Format(Properties.Resources.Notice_NoPages, book.Path)));

                    if (Config.Current.Book.IsConfirmRecursive && (args.Option & BookLoadOption.ReLoad) == 0 && !book.Source.IsRecursiveFolder && book.Source.SubFolderCount > 0)
                    {
                        AppDispatcher.Invoke(() => ConfirmRecursive(args.Sender, book, token));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // nop.
            }
            catch (Exception ex)
            {
                if (ex is BookAddressException)
                {
                    EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(ex.Message));
                }
                else
                {
                    // ファイル読み込み失敗通知
                    var message = string.Format(Properties.Resources.LoadFailedException_Message, place, ex.Message);
                    EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(message));
                }

                // 現在表示されているコンテンツを無効
                ViewContentsChanged?.Invoke(args.Sender, new ViewContentSourceCollectionChangedEventArgs());

                // 本の変更通知
                BookChanged?.Invoke(this, new BookChangedEventArgs(Address, null, BookMementoType.None));

                // 履歴リスト更新
                if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                {
                    HistoryListSync?.Invoke(this, new BookHubPathEventArgs(Address));
                }
            }
            finally
            {
                // Now Loading OFF
                NotifyLoading(null);

                ////DebugTimer.Check("Done.");
            }
        }

        /// <summary>
        /// BookMementの管理者を取得
        /// </summary>
        private static BookMementoType GetBookMementoType(Book book)
        {
            if (book is null) return BookMementoType.None;

            if (BookmarkCollection.Current.Contains(book.Path))
            {
                return BookMementoType.Bookmark;
            }
            else if (BookHistoryCollection.Current.Contains(book.Path))
            {
                return BookMementoType.History;
            }
            else
            {
                return BookMementoType.None;
            }
        }

        // 再帰読み込み確認
        // TODO: UI操作を行うのはいかがなものか。イベントかMessengerにする必要あり。
        private void ConfirmRecursive(object? sender, Book book, CancellationToken token)
        {
            if (book is null) throw new ArgumentNullException(nameof(book));

            token.ThrowIfCancellationRequested();

            var dialog = new MessageDialog(string.Format(Properties.Resources.ConfirmRecursiveDialog_Message, book.Path), Properties.Resources.ConfirmRecursiveDialog_Title);
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);

            using (var register = token.Register(() => dialog.Close()))
            {
                var result = dialog.ShowDialog();
                token.ThrowIfCancellationRequested();

                if (result.Command == UICommands.Yes)
                {
                    RequestReloadRecursive(sender, book);
                }
            }
        }

        private void RequestReloadRecursive(object? sender, Book book)
        {
            // TODO: BookAddressそのまま渡せばよいと思う
            RequestLoad(sender, book.Path, book.StartEntry, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
        }

        /// <summary>
        /// ブックを読み込む(本体)
        /// </summary>
        private static async Task<Book> LoadAsyncCore(object? sender, BookAddress address, BookLoadOption option, BookMemento setting, bool isNew, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var memento = ((option & BookLoadOption.ReLoad) == BookLoadOption.ReLoad) ? BookSettingPresenter.Current.LatestSetting.ToBookMemento() : setting;

            var bookSetting = new BookCreateSetting()
            {
                StartPage = BookLoadOptionHelper.CreateBookStartPage(address.EntryName, option),
                IsRecursiveFolder = BookLoadOptionHelper.CreateIsRecursiveFolder(memento.IsRecursiveFolder, option),
                ArchiveRecursiveMode = Config.Current.System.ArchiveRecursiveMode,
                BookPageCollectMode = Config.Current.System.BookPageCollectMode,
                SortMode = memento.SortMode,
                IsIgnoreCache = option.HasFlag(BookLoadOption.IgnoreCache),
                IsNew = isNew,
                LoadOption = option,
            };

            var book = await BookFactory.CreateAsync(sender, address, bookSetting, memento, token);

            // auto recursive
            if (Config.Current.Book.IsAutoRecursive && !book.Source.IsRecursiveFolder && book.Source.SubFolderCount == 1)
            {
                bookSetting.IsRecursiveFolder = true;
                book = await BookFactory.CreateAsync(sender, address, bookSetting, memento, token);
            }

            return book;
        }

        /// <summary>
        /// ブックの動作開始と最初のページの表示を待つ
        /// </summary>
        private async Task WaitBookReadyAsync(Book book, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (book is null) return;

            var tcs = new TaskCompletionSource<bool>();

            using (book.Viewer.Loader.SubscribeViewContentsChanged(BookViewer_ViewContentsChangedInner))
            {
                // ブックエンジン開始
                book.Start();

                // 最初のコンテンツ表示待ち
                if (book.Pages.Count > 0)
                {
                    using (var register = token.Register(() => tcs.TrySetCanceled()))
                    {
                        await tcs.Task;
                        token.ThrowIfCancellationRequested();
                    }
                }
            }

            //// inner callback function define.
            void BookViewer_ViewContentsChangedInner(object? s, ViewContentSourceCollectionChangedEventArgs e)
            {
                if (e.ViewPageCollection.Collection.All(x => x.Content.IsViewReady))
                {
                    tcs.TrySetResult(true);
                }
            }
        }

        /// <summary>
        /// ブックイベント購読
        /// </summary>
        private void SubscribeBook(Book book)
        {
            if (book is null) return;

            book.Viewer.Loader.ViewContentsChanged += BookViewer_ViewContentsChanged;
            book.Viewer.Loader.NextContentsChanged += BookViewer_NextContentsChanged;
            book.Source.DartyBook += BookSource_DartyBook;
        }

        /// <summary>
        /// ブックイベント購読解除
        /// </summary>
        private void UnsubscribeBook(Book book)
        {
            if (book is null) return;

            book.Viewer.Loader.ViewContentsChanged -= BookViewer_ViewContentsChanged;
            book.Viewer.Loader.NextContentsChanged -= BookViewer_NextContentsChanged;
            book.Source.DartyBook -= BookSource_DartyBook;
        }

        #endregion BookHubCommand.Load

        #region BookHubCommand.Unload

        /// <summary>
        /// 本の開放
        /// </summary>
        public void Unload(BookHubCommandUnloadArgs param)
        {
            if (_disposedValue) return;

            // 本の変更通
            BookChanging?.Invoke(param.Sender, new BookChangingEventArgs(""));

            UnloadCore();

            if (param.IsClearViewContent)
            {
                this.Address = null;

                // 現在表示されているコンテンツを無効
                ViewContentsChanged?.Invoke(param.Sender, new ViewContentSourceCollectionChangedEventArgs());
            }

            if (param.Message != null)
            {
                EmptyPageMessage?.Invoke(this, new BookHubMessageEventArgs(param.Message));
            }

            // 本の変更通
            BookChanged?.Invoke(param.Sender, new BookChangedEventArgs("", null, BookMementoType.None));
        }

        /// <summary>
        /// 本の開放
        /// </summary>
        private void UnloadCore()
        {
            // 再生中のメディアをPAUSE
            // TODO: ここでUI操作を行うのはいかがなものか。イベントかMessengerにする必要あり。
            AppDispatcher.Invoke(() => MediaPlayerOperator.Current?.Pause());

            var book = _book;
            _book = null;

            if (book is not null)
            {
                // 履歴の保存
                SaveBookMemento(book);

                // 現在の本を開放
                UnsubscribeBook(book);
                book.Dispose();
            }
        }

        #endregion BookHubCommand.Unload

        #region BookMemento Control

        /// <summary>
        /// 現在開いているブックの設定作成
        /// </summary>
        private static BookMemento? CreateBookMemento(Book book)
        {
            return (book != null && book.Pages.Count > 0) ? book.CreateMemento() : null;
        }

        // ブック設定の作成
        // 開いているブックならばその設定を取得する
        public BookMemento CreateBookMemento(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var book = _book;
            var memento = book is not null ? CreateBookMemento(book) : null;
            if (memento == null || memento.Path != path)
            {
                memento = BookSettingPresenter.Current.DefaultSetting.ToBookMemento();
                memento.Path = path;
            }
            return memento;
        }

        /// <summary>
        /// 最新の設定を取得
        /// </summary>
        /// <param name="path">場所</param>
        /// <param name="lastest">現在の情報</param>
        private static BookMemento? CreateLastestBookMemento(string path, BookMemento? lastest)
        {
            BookMemento? memento = null;

            if (lastest?.Path == path)
            {
                memento = lastest.Clone();
            }
            else
            {
                var unit = BookMementoCollection.Current.GetValid(path);
                if (unit != null)
                {
                    memento = unit.Memento.Clone();
                }
            }

            return memento;
        }

        /// <summary>
        /// 適切な設定を作成
        /// </summary>
        /// <param name="path">場所</param>
        /// <param name="lastest">現在の情報</param>
        /// <param name="option">読み込みオプション</param>
        public static BookMemento CreateOpenBookMemento(string path, BookMemento? lastest, BookLoadOption option)
        {
            var memory = CreateLastestBookMemento(path, lastest);
            Debug.Assert(memory == null || memory.Path == path);

            if (memory != null && option.HasFlag(BookLoadOption.Resume))
            {
                return memory.Clone();
            }
            else
            {
                var restore = BookSettingConfigExtensions.FromBookMement(memory);
                return BookSettingPresenter.Current.GetSetting(restore, option.HasFlag(BookLoadOption.DefaultRecursive)).ToBookMemento();
            }
        }

        // 設定の保存
        public void SaveBookMemento()
        {
            if (_disposedValue) return;

            var book = _book;
            if (book is null) return;

            SaveBookMemento(book);
        }

        //設定の保存
        private void SaveBookMemento(Book book)
        {
            if (_disposedValue) return;
            if (book is null) return;

            var memento = CreateBookMemento(book);
            if (memento is null) return;

            bool isKeepHistoryOrder = book.IsKeepHistoryOrder || Config.Current.History.IsForceUpdateHistory;
            SaveBookMemento(book, memento, isKeepHistoryOrder);
        }

        private void SaveBookMemento(Book book, BookMemento memento, bool isKeepHistoryOrder)
        {
            if (_disposedValue) return;
            if (memento == null) return;

            // ブックマークの更新
            BookmarkCollection.Current.Update(memento, book.Viewer.PageChangeCount > 1);

            // 履歴の保存
            if (CanHistory(book))
            {
                BookHistoryCollection.Current.Add(memento, isKeepHistoryOrder);
            }
        }

        /// <summary>
        /// 記録のページのみクリア
        /// </summary>
        private void ResetBookMementoPage(string place)
        {
            if (place is null) throw new ArgumentNullException(nameof(place));

            if (_disposedValue) return;

            var unit = BookMementoCollection.Current.GetValid(place);
            if (unit?.Memento != null)
            {
                unit.Memento.Page = "";
            }
        }

        /// <summary>
        /// 最新の本の設定を取得
        /// </summary>
        /// <param name="address">場所</param>
        /// <param name="option"></param>
        /// <returns></returns>
        public BookMemento GetLastestBookMemento(string address, BookLoadOption option)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));

            var book = _book;
            if (book is not null && book.Path == address)
            {
                return book.CreateMemento();
            }

            return CreateOpenBookMemento(address, null, option);
        }

        // 履歴登録可
        private bool CanHistory(Book book)
        {
            if (_disposedValue) return false;

            if (book is null) return false;

            // 履歴閲覧時の履歴更新は最低１操作を必要とする
            var historyEntryPageCount = Config.Current.History.HistoryEntryPageCount;
            if (book.IsKeepHistoryOrder && Config.Current.History.IsForceUpdateHistory && historyEntryPageCount <= 0)
            {
                historyEntryPageCount = 1;
            }

            return !_historyRemoved
                && book.Pages.Count > 0
                && (_historyEntry || book.Viewer.PageChangeCount > historyEntryPageCount || book.Viewer.IsPageTerminated)
                && (Config.Current.History.IsInnerArchiveHistoryEnabled || book.Source.ArchiveEntryCollection.Archiver?.Parent == null)
                && (Config.Current.History.IsUncHistoryEnabled || !LoosePath.IsUnc(book.Path));
        }

        #endregion BookMemento Control

        #region IDisposable Support
        private bool _disposedValue = false;

        private void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();

                    // reset event
                    // 非同期で処理されるため、イベントを確実に停止させるためにクリアしている
                    this.BookChanging = null;
                    this.BookChanged = null;
                    this.LoadRequested = null;
                    this.Loading = null;
                    this.ViewContentsChanged = null;
                    this.NextContentsChanged = null;
                    this.EmptyMessage = null;
                    this.EmptyPageMessage = null;
                    this.FolderListSync = null;
                    this.HistoryListSync = null;
                    this.HistoryChanged = null;
                    this.BookmarkChanged = null;
                    this.AddressChanged = null;
                    ResetPropertyChanged();

                    SaveBookMemento();

                    _book?.Dispose();

                    Debug.WriteLine("BookHub Disposed.");
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}

