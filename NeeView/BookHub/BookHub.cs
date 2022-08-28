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

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            public static extern int GetLongPathName(string shortPath, StringBuilder longPath, int longPathLength);
        }

        // パス名の正規化
        private static string GetNormalizePathName(string source)
        {
            // 区切り文字修正
            source = new System.Text.RegularExpressions.Regex(@"[/\\]").Replace(source, "\\").TrimEnd('\\');

            // ドライブレター修正
            source = new System.Text.RegularExpressions.Regex(@"^[a-z]:").Replace(source, m => m.Value.ToUpper());
            source = new System.Text.RegularExpressions.Regex(@":$").Replace(source, ":\\");

            StringBuilder longPath = new StringBuilder(1024);
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
        private BookUnit? _bookUnit;
        private string? _address;
        private BookHubCommandEngine _commandEngine;
        private bool _historyEntry;
        private bool _historyRemoved;
        private int _requestLoadCount;
        private object _lock = new object();
        private DisposableCollection _disposables;

        private BookHub()
        {
            _disposables = new DisposableCollection();

            _disposables.Add(SubscribeBookChanged(
                (s, e) =>
                {
                    if (_disposedValue) return;

                    if (this.Book?.NotFoundStartPage != null && this.Book.Pages.Count > 0)
                    {
                        InfoMessage.Current.SetMessage(InfoMessageType.BookName, string.Format(Properties.Resources.Notice_CannotOpen, LoosePath.GetFileName(this.Book.NotFoundStartPage)), null, 2.0);
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
                (s, e) => Book?.Restore(BookSettingPresenter.Current.LatestSetting.ToBookMemento())));

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



        /// <summary>
        /// 現在の本
        /// </summary>
        public BookUnit? BookUnit
        {
            get { return _bookUnit; }
            private set { SetProperty(ref _bookUnit, value); }
        }

        // 現在の本
        public Book? Book => BookUnit?.Book;

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


        #region Callback Methods

        private void BookHistoryCollection_HistoryChanged(object? sender, BookMementoCollectionChangedArgs e)
        {
            if (_disposedValue) return;

            HistoryChanged?.Invoke(sender, e);

            lock (_lock)
            {
                if (BookUnit == null) return;

                // 履歴削除されたものを履歴登録しないようにする
                if (e.HistoryChangedType == BookMementoCollectionChangedType.Remove && (this.BookUnit.Book.Address == e.Key || e.Key == null))
                {
                    _historyRemoved = true;
                }
            }
        }

        private void BookViewer_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            AppDispatcher.Invoke(() =>
            {
                bool allowUpdateHistory;
                lock (_lock)
                {
                    _historyRemoved = false;

                    if (BookUnit == null) return;
                    allowUpdateHistory = !BookUnit.IsKeepHistoryOrder || Config.Current.History.IsForceUpdateHistory;
                }

                // 履歴更新
                if (allowUpdateHistory && !_historyEntry && CanHistory())
                {
                    _historyEntry = true;
                    var memento = Book?.CreateMemento();
                    if (memento is not null)
                    {
                        BookHistoryCollection.Current.Add(memento, false);
                    }
                }

                lock (_lock)
                {
                    if (BookUnit == null) return;
                    var viewPages = e.ViewPageCollection?.Collection.Where(x => x != null).Select(x => x.Page).ToList() ?? new List<Page>();
                    BookUnit.Book.Pages.SetViewPageFlag(viewPages);
                }

                ViewContentsChanged?.Invoke(sender, e);
            });
        }

        private void BookViewer_NextContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            if (_disposedValue) return;

            if (BookUnit == null) return;
            NextContentsChanged?.Invoke(sender, e);
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

            if (Book?.Address == path && option.HasFlag(BookLoadOption.SkipSamePlace)) return null;

            Address = path;

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

            return BookUnit != null || _commandEngine.IsBusy;
        }

        /// <summary>
        /// リクエスト：フォルダーを閉じる
        /// </summary>
        /// <param name="isClearViewContent"></param>
        /// <returns></returns>
        public BookHubCommandUnload RequestUnload(object sender, bool isClearViewContent, string? message = null)
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
            if (_disposedValue) return;

            if (_isLoading || Address == null) return;

            BookLoadOption options;
            lock (_lock)
            {
                options = BookUnit != null ? (BookUnit.LoadOptions & BookLoadOption.KeepHistoryOrder) | BookLoadOption.Resume : BookLoadOption.None;
            }
            RequestLoad(sender, Address, null, options | BookLoadOption.IsBook | BookLoadOption.IgnoreCache, true);
        }

        // 上の階層に移動可能？
        public bool CanLoadParent()
        {
            if (_disposedValue) return false;

            var parent = BookUnit?.BookAddress?.Place;
            return parent?.Path != null && parent.Scheme == QueryScheme.File;
        }

        /// <summary>
        /// リクエスト：上の階層に移動
        /// </summary>
        public void RequestLoadParent(object sender)
        {
            if (_disposedValue) return;

            var bookAddress = BookUnit?.BookAddress;

            var current = bookAddress?.Address;
            if (current is null) return;

            var parent = bookAddress?.Place;
            if (parent is null) return;

            if (parent.Path != null && parent.Scheme == QueryScheme.File)
            {
                var entryName = current.SimplePath.Substring(parent.SimplePath.Length).TrimStart(LoosePath.Separators);
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
            AppDispatcher.Invoke(() => Loading?.Invoke(this, new BookHubPathEventArgs(path)));
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
            var lastBookMemento = this.Book?.Address != null ? this.Book.CreateMemento() : null;

            Address = args.SourcePath;

            AppDispatcher.Invoke(() =>
            {
                // 本の変更開始通知
                // NOTE: パスはまだページの可能性があるので不完全
                BookChanging?.Invoke(this, new BookChangingEventArgs(Address));
            });

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
                    AppDispatcher.Invoke(() => HistoryListSync?.Invoke(this, new BookHubPathEventArgs(address.Address.SimplePath)));
                }

                // 本の設定
                var setting = CreateOpenBookMemento(address.Address.SimplePath, lastBookMemento, args.Option);

                // 最終ページなら初期化？
                bool isResetLastPage = address.EntryName == null && Config.Current.BookSettingPolicy.Page == BookSettingPageSelectMode.RestoreOrDefaultReset;

                address.EntryName = address.EntryName ?? LoosePath.NormalizeSeparator(setting.Page);
                place = address.SystemPath;

                // 移動履歴登録
                BookHubHistory.Current.Add(args.Sender, address.Address);

                // フォルダーリスト更新
                if (args.IsRefreshFolderList)
                {
                    AppDispatcher.Invoke(() => FolderListSync?.Invoke(this, new FolderListSyncEventArgs(address.Address.SimplePath, address.Place.SimplePath, false)));
                }

                var isNew = BookMementoCollection.Current.GetValid(address.Address.SimplePath) == null;

                // Load本体
                var loadOption = args.Option | (isResetLastPage ? BookLoadOption.ResetLastPage : BookLoadOption.None);
                var bookUnit = await LoadAsyncCore(args.Sender, address, loadOption, setting, isNew, token);

                _historyEntry = false;
                _historyRemoved = false;

                var bookSetting = BookSettingConfigExtensions.FromBookMement(bookUnit.Book.CreateMemento());
                if (bookSetting is not null)
                {
                    AppDispatcher.Invoke(() =>
                    {
                        // 本の設定を更新
                        BookSettingPresenter.Current.SetLatestSetting(bookSetting);
                    });
                }

                lock (_lock)
                {
                    BookUnit = bookUnit;
                    Address = bookUnit.Book.Address;
                }

                await WaitBookReadyAsync(token);

                ////DebugTimer.Check("LoadCore");

                AppDispatcher.Invoke(() =>
                {
                    lock (_lock)
                    {
                        BookChanged?.Invoke(this, new BookChangedEventArgs(bookUnit.Book.Address, bookUnit.Book, bookUnit.GetBookMementoType()));
                    }
                });

                // ページがなかった時の処理
                if (Book != null && Book.Pages.Count <= 0)
                {
                    ResetBookMementoPage(Book.Address);

                    AppDispatcher.Invoke(() =>
                    {
                        EmptyMessage?.Invoke(this, new BookHubMessageEventArgs(string.Format(Properties.Resources.Notice_NoPages, Book.Address)));

                        if (Config.Current.Book.IsConfirmRecursive && (args.Option & BookLoadOption.ReLoad) == 0 && !Book.Source.IsRecursiveFolder && Book.Source.SubFolderCount > 0)
                        {
                            ConfirmRecursive(args.Sender, token);
                        }
                    });
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

                AppDispatcher.Invoke(() =>
                {
                    // 現在表示されているコンテンツを無効
                    ViewContentsChanged?.Invoke(args.Sender, new ViewContentSourceCollectionChangedEventArgs("", new ViewContentSourceCollection()));

                    // 本の変更通知
                    BookChanged?.Invoke(this, new BookChangedEventArgs(Address, null, BookMementoType.None));

                    // 履歴リスト更新
                    if ((args.Option & BookLoadOption.SelectHistoryMaybe) != 0)
                    {
                        HistoryListSync?.Invoke(this, new BookHubPathEventArgs(Address));
                    }
                });
            }
            finally
            {
                // Now Loading OFF
                NotifyLoading(null);

                ////DebugTimer.Check("Done.");
            }
        }

        // 再帰読み込み確認
        private void ConfirmRecursive(object? sender, CancellationToken token)
        {
            if (Book is null) throw new InvalidOperationException();

            token.ThrowIfCancellationRequested();

            var dialog = new MessageDialog(string.Format(Properties.Resources.ConfirmRecursiveDialog_Message, Book.Address), Properties.Resources.ConfirmRecursiveDialog_Title);
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);

            using (var register = token.Register(() => dialog.Close()))
            {
                var result = dialog.ShowDialog();
                token.ThrowIfCancellationRequested();

                if (result == UICommands.Yes)
                {
                    RequestReloadRecursive(sender, Book);
                }
            }
        }

        private void RequestReloadRecursive(object? sender, Book book)
        {
            // TODO: BookAddressそのまま渡せばよいと思う
            RequestLoad(sender, book.Address, book.StartEntry, BookLoadOption.Recursive | BookLoadOption.ReLoad, true);
        }

        /// <summary>
        /// 本を読み込む(本体)
        /// </summary>
        private async Task<BookUnit> LoadAsyncCore(object? sender, BookAddress address, BookLoadOption option, Book.Memento setting, bool isNew, CancellationToken token)
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

            var book = await BookFactory.CreateAsync(sender, address.Address, address.SourceAddress, bookSetting, memento, token);

            // auto recursive
            if (Config.Current.Book.IsAutoRecursive && !book.Source.IsRecursiveFolder && book.Source.SubFolderCount == 1)
            {
                bookSetting.IsRecursiveFolder = true;
                book = await BookFactory.CreateAsync(sender, address.Address, address.SourceAddress, bookSetting, memento, token);
            }

            return new BookUnit(book, address, option);
        }

        private async Task WaitBookReadyAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (BookUnit is null) return;

            var book = BookUnit.Book;

            // イベント設定
            // NOTE: book のほうが BookHub より寿命が短いため開放は考えなくてよい？どうだろう？
            // BookUnit で管理すべき？
            book.Viewer.ViewContentsChanged += BookViewer_ViewContentsChanged;
            book.Viewer.NextContentsChanged += BookViewer_NextContentsChanged;
            book.Source.DartyBook += (s, e) => RequestLoad(this, Address, null, BookLoadOption.ReLoad | BookLoadOption.IsBook, false);

            var tcs = new TaskCompletionSource<bool>();

            using (book.Viewer.SubscribeViewContentsChanged(BookViewer_ViewContentsChangedInner))
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

        #endregion BookHubCommand.Load

        #region BookHubCommand.Unload

        /// <summary>
        /// 本の開放
        /// </summary>
        public void Unload(BookHubCommandUnloadArgs param)
        {
            if (_disposedValue) return;

            AppDispatcher.Invoke(() =>
            {
                // 本の変更通
                BookChanging?.Invoke(param.Sender, new BookChangingEventArgs(""));
            });

            UnloadCore();

            if (param.IsClearViewContent)
            {
                Address = null;

                AppDispatcher.Invoke(() =>
                {
                    // 現在表示されているコンテンツを無効
                    ViewContentsChanged?.Invoke(param.Sender, new ViewContentSourceCollectionChangedEventArgs());
                });
            }

            if (param.Message != null)
            {
                EmptyPageMessage?.Invoke(this, new BookHubMessageEventArgs(param.Message));
            }

            AppDispatcher.Invoke(() =>
            {
                // 本の変更通
                BookChanged?.Invoke(param.Sender, new BookChangedEventArgs("", null, BookMementoType.None));
            });
        }

        /// <summary>
        /// 本の開放
        /// </summary>
        private void UnloadCore()
        {
            AppDispatcher.Invoke(() =>
            {
                // 再生中のメディアをPAUSE
                MediaPlayerOperator.Current?.Pause();
            });

            // 履歴の保存
            SaveBookMemento();

            // 現在の本を開放
            lock (_lock)
            {
                BookUnit?.Dispose();
                BookUnit = null;
            }
        }

        #endregion BookHubCommand.Unload

        #region BookMemento Control

        //現在開いているブックの設定作成
        public Book.Memento? CreateBookMemento()
        {
            ThrowIfDisposed();

            lock (_lock)
            {
                return (BookUnit != null && BookUnit.Book.Pages.Count > 0) ? BookUnit.Book.CreateMemento() : null;
            }
        }

        // ブック設定の作成
        // 開いているブックならばその設定を取得する
        public Book.Memento CreateBookMemento(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(Path));

            ThrowIfDisposed();

            var memento = CreateBookMemento();
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
        private static Book.Memento? CreateLastestBookMemento(string path, Book.Memento? lastest)
        {
            Book.Memento? memento = null;

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
        public static Book.Memento CreateOpenBookMemento(string path, Book.Memento? lastest, BookLoadOption option)
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

        //設定の保存
        public void SaveBookMemento()
        {
            if (_disposedValue) return;

            var memento = CreateBookMemento();
            if (memento == null) return;

            bool isKeepHistoryOrder;
            lock (_lock)
            {
                if (BookUnit == null) return;
                isKeepHistoryOrder = BookUnit.IsKeepHistoryOrder || Config.Current.History.IsForceUpdateHistory;
            }
            SaveBookMemento(memento, isKeepHistoryOrder);
        }

        private void SaveBookMemento(Book.Memento memento, bool isKeepHistoryOrder)
        {
            if (memento == null) return;

            if (_disposedValue) return;

            // 情報更新
            var unit = BookMementoCollection.Current.Get(memento.Path);
            if (unit != null)
            {
                unit.Memento = memento;
            }

            // 履歴の保存
            if (CanHistory())
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
                unit.Memento.Page = null;
            }
        }

        /// <summary>
        /// 最新の本の設定を取得
        /// </summary>
        /// <param name="address">場所</param>
        /// <param name="option"></param>
        /// <returns></returns>
        public Book.Memento GetLastestBookMemento(string address, BookLoadOption option)
        {
            if (address is null) throw new ArgumentNullException(nameof(address));

            ThrowIfDisposed();

            var book = this.Book;
            if (book is not null && book.Address == address)
            {
                return book.CreateMemento();
            }

            return CreateOpenBookMemento(address, null, option);
        }

        // 履歴登録可
        private bool CanHistory()
        {
            if (_disposedValue) return false;

            // 履歴閲覧時の履歴更新は最低１操作を必要とする
            var historyEntryPageCount = Config.Current.History.HistoryEntryPageCount;
            if (BookUnit != null && BookUnit.IsKeepHistoryOrder && Config.Current.History.IsForceUpdateHistory && historyEntryPageCount <= 0)
            {
                historyEntryPageCount = 1;
            }

            return Book != null
                && !_historyRemoved
                && Book.Pages.Count > 0
                && (_historyEntry || Book.Viewer.PageChangeCount > historyEntryPageCount || Book.Viewer.IsPageTerminated)
                && (Config.Current.History.IsInnerArchiveHistoryEnabled || Book.Source.ArchiveEntryCollection.Archiver?.Parent == null)
                && (Config.Current.History.IsUncHistoryEnabled || !LoosePath.IsUnc(Book.Address));
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

                    this.BookUnit?.Dispose();

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

        #region Memento

        /// <summary>
        /// BookHub Memento
        /// </summary>
        [DataContract]
        public class Memento : BindableBase, IMemento
        {
            [DataMember]
            public int _Version { get; set; } = Environment.ProductVersionNumber;

            [DataMember(Order = 6)]
            public bool IsConfirmRecursive { get; set; }

            [DataMember(Order = 10)]
            public bool IsAutoRecursive { get; set; }

            [DataMember, DefaultValue(0)]
            public int HistoryEntryPageCount { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInnerArchiveHistoryEnabled { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsUncHistoryEnabled { get; set; }

            [DataMember]
            public bool IsForceUpdateHistory { get; set; }

            [DataMember, DefaultValue(ArchiveEntryCollectionMode.IncludeSubArchives)]
            public ArchiveEntryCollectionMode ArchiveRecursveMode { get; set; }

            #region Obslete

            [Obsolete, DataMember(Order = 22, EmitDefaultValue = false)]
            public bool IsAutoRecursiveWithAllFiles { get; set; } // no used (ver.34)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsArchiveRecursive { get; set; } // no used (ver.34)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnableAnimatedGif { get; set; }

            [Obsolete, DataMember(Order = 1, EmitDefaultValue = false)]
            public bool IsEnableExif { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnableNoSupportFile { get; set; }

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public PreLoadMode PreLoadMode { get; set; }

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsEnabledAutoNextFolder { get; set; } // no used

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public PageEndAction PageEndAction { get; set; } // no used (ver.23)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public bool IsSlideShowByLoop { get; set; } // no used (ver.22)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public double SlideShowInterval { get; set; } // no used (ver.22)

            [Obsolete, DataMember(Order = 7, EmitDefaultValue = false)]
            public bool IsCancelSlideByMouseMove { get; set; } // no used (ver.22)

            [Obsolete, DataMember(EmitDefaultValue = false)]
            public Book.Memento? BookMemento { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 2, EmitDefaultValue = false)]
            public bool IsEnarbleCurrentDirectory { get; set; } // no used

            [Obsolete, DataMember(Order = 4, EmitDefaultValue = false)]
            public bool IsSupportArchiveFile { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 5, EmitDefaultValue = false)]
            public bool AllowPagePreLoad { get; set; } // no used

            [Obsolete, DataMember(Order = 6, EmitDefaultValue = false)]
            public Book.Memento? BookMementoDefault { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 6, EmitDefaultValue = false)]
            public bool IsUseBookMementoDefault { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 19, EmitDefaultValue = false)]
            public BookMementoFilter? HistoryMementoFilter { get; set; } // no used (v.23)

            [Obsolete, DataMember(Order = 20, EmitDefaultValue = false)]
            public string? Home { get; set; } // no used (ver.23)

            #endregion

            [OnDeserializing]
            private void OnDeserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }

            [OnDeserialized]
            private void OnDeserialized(StreamingContext c)
            {
#pragma warning disable CS0612
                // before 34.0
                if (_Version < Environment.GenerateProductVersionNumber(34, 0, 0))
                {
                    ArchiveRecursveMode = IsArchiveRecursive ? ArchiveEntryCollectionMode.IncludeSubArchives : ArchiveEntryCollectionMode.IncludeSubDirectories;
                }
#pragma warning restore CS0612
            }

            public void RestoreConfig(Config config)
            {
                config.System.ArchiveRecursiveMode = ArchiveRecursveMode;
                config.Book.IsConfirmRecursive = IsConfirmRecursive;
                config.Book.IsAutoRecursive = IsAutoRecursive;
                config.History.HistoryEntryPageCount = HistoryEntryPageCount;
                config.History.IsInnerArchiveHistoryEnabled = IsInnerArchiveHistoryEnabled;
                config.History.IsUncHistoryEnabled = IsUncHistoryEnabled;
                config.History.IsForceUpdateHistory = IsForceUpdateHistory;
            }
        }

        #endregion
    }
}

