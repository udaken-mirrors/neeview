using NeeLaboratory.ComponentModel;
using NeeLaboratory.Threading.Jobs;
using System;
using System.Threading;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 本の操作
    /// </summary>
    public class BookOperation : BindableBase
    {
        // System Object
        static BookOperation() => Current = new BookOperation();
        public static BookOperation Current { get; }


        private BookHub _bookHub;
        private Book? _book;
        private bool _isLoading;

        public BookPageTerminatorProxy _terminator = new();
        public BookPageScriptProxy _script = new();
        public BookControlProxy _bookControl = new();
        public BookPageControlProxy _control = new();
        public BookPlaylistControlProxy _playlist = new();


        private BookOperation()
        {
            _bookHub = BookHub.Current;

            _bookHub.BookChanging +=
                (s, e) => AppDispatcher.Invoke(() => BookHub_BookChanging(s, e));

            _bookHub.BookChanged +=
                (s, e) => AppDispatcher.Invoke(() => BookHub_BookChanged(s, e));
        }



        // ブックが変更される
        public event EventHandler<BookChangingEventArgs>? BookChanging;

        public IDisposable SubscribeBookChanging(EventHandler<BookChangingEventArgs> handler)
        {
            BookChanging += handler;
            return new AnonymousDisposable(() => BookChanging -= handler);
        }

        // ブックが変更された
        public event EventHandler<BookChangedEventArgs>? BookChanged;

        public IDisposable SubscribeBookChanged(EventHandler<BookChangedEventArgs> handler)
        {
            BookChanged += handler;
            return new AnonymousDisposable(() => BookChanged -= handler);
        }



        public bool IsLoading => _bookHub.IsLoading || _isLoading;

        public Book? Book => _book;

        public string? Address => Book?.Path;

        public BookControlProxy BookControl => _bookControl;

        public BookPlaylistControlProxy Playlist => _playlist;

        public BookPageControlProxy Control => _control;



        private void BookHub_BookChanging(object? sender, BookChangingEventArgs e)
        {
            _isLoading = true;
            SetBook(null);
            BookChanging?.Invoke(sender, e);
        }

        private void BookHub_BookChanged(object? sender, BookChangedEventArgs e)
        {
            SetBook(_bookHub.GetCurrentBook());
            _isLoading = false;
            BookChanged?.Invoke(sender, e);
        }

        private void SetBook(Book? book)
        {
            if (_book == book) return;
            _book = book;

            _bookControl.SetSource(CreateBoolController(_book));
            _control.SetSource(CreateController(_book));
            _playlist.SetSource(CreatePlaylistController(_book));
            _terminator.SetSource(CreatePageTerminator(_book));
            _script.SetSource(CreateBookScript(_book));

            RaisePropertyChanged(nameof(Book));
            RaisePropertyChanged(nameof(Address));
        }

        private BookPageTerminator? CreatePageTerminator(Book? book)
        {
            return book is null ? null : new BookPageTerminator(book, _control);
        }

        private BookPageScript? CreateBookScript(Book? book)
        {
            return book is null ? null : new BookPageScript(book);
        }

        private IBookControl? CreateBoolController(Book? book)
        {
            return book is null ? null : new BookControl(book);
        }

        private IBookPageControl? CreateController(Book? book)
        {
            return book is null ? null : new BookPageControl(book, _bookControl);
        }

        private BookPlaylistControl? CreatePlaylistController(Book? book)
        {
            return (book is null || book.IsMedia) ? null : new BookPlaylistControl(book);
        }



        #region BookCommand : ページ操作

        // パスを指定して移動
        public bool JumpPageWithPath(object? sender, string path)
        {
            if (this.Book == null || this.Book.IsMedia) return false;

            var page = this.Book.Pages.GetPageWithEntryFullName(path);
            if (page is null) return false;
            _control.MoveTo(sender, page.Index);
            return true;
        }

        // ページを指定して移動
        public void JumpPageAs(object? sender)
        {
            if (this.Book == null || this.Book.IsMedia) return;

            var dialogModel = new PageSelecteDialogModel(this.Book.Viewer.GetViewPageIndex() + 1, 1, this.Book.Pages.Count);

            var dialog = new PageSelectDialog(dialogModel);
            dialog.Owner = MainViewComponent.Current.GetWindow();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var result = dialog.ShowDialog();

            if (result == true)
            {
                var page = this.Book.Pages.GetPage(dialogModel.Value - 1);
                if (page is null) return;
                _control.MoveTo(sender, page.Index);
            }
        }

        // 指定ページに移動
        public void JumpPage(object? sender, Page? page)
        {
            if (page is null) return;
            _control.MoveTo(sender, page.Index);
        }

        #endregion

        #region BookCommand : メディア操作

        // 動画再生中？
        public bool IsMediaPlaying()
        {
            if (MediaPlayerOperator.Current is null) return false;

            if (this.Book != null && this.Book.IsMedia)
            {
                return MediaPlayerOperator.Current.IsPlaying;
            }
            else
            {
                return false;
            }
        }

        // 動画再生ON/OFF
        public bool ToggleMediaPlay()
        {
            if (MediaPlayerOperator.Current is null) return false;

            if (this.Book != null && this.Book.IsMedia)
            {
                if (MediaPlayerOperator.Current.IsPlaying)
                {
                    MediaPlayerOperator.Current.Pause();
                }
                else
                {
                    MediaPlayerOperator.Current.Play();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region BookCommand : 下位ブックに移動

        public bool CanMoveToChildBook()
        {
            var page = Book?.Viewer.GetViewPage();
            return page != null && page.PageType == PageType.Folder;
        }

        public void MoveToChildBook(object sender)
        {
            var page = Book?.Viewer.GetViewPage();
            if (page != null && page.PageType == PageType.Folder)
            {
                _bookHub.RequestLoad(sender, page.Entry.SystemPath, null, BookLoadOption.IsBook | BookLoadOption.SkipSamePlace, true);
            }
        }

        #endregion

        #region ページ読み込み完了待機

        /// <summary>
        /// 表示ページ読み込み完了まで待機
        /// </summary>
        public void Wait(CancellationToken token)
        {
            BookHub bookHub = BookHub.Current;

            var book = bookHub.GetCurrentBook();
            if (!bookHub.IsBusy && book?.Control.IsViewContentsLoading != true)
            {
                return;
            }

            // BookHubのコマンド処理が終わるまで待機
            var eventFlag = new ManualResetEventSlim();
            bookHub.IsBusyChanged += BookHub_IsBusyChanged;
            try
            {
                if (bookHub.IsBusy)
                {
                    eventFlag.Wait(token);
                }
            }
            finally
            {
                bookHub.IsBusyChanged -= BookHub_IsBusyChanged;
            }

            book = bookHub.GetCurrentBook();
            if (book is null)
            {
                return;
            }

            // 表示ページの読み込みが終わるまで待機
            eventFlag.Reset();
            bool _isBookChanged = false;
            bookHub.BookChanged += BookOperation_BookChanged;
            book.Control.ViewContentsLoading += BookControl_ViewContentsLoading;
            try
            {
                if (book.Control.IsViewContentsLoading)
                {
                    eventFlag.Wait(token);
                }
            }
            finally
            {
                bookHub.BookChanged -= BookOperation_BookChanged;
                book.Control.ViewContentsLoading -= BookControl_ViewContentsLoading;
            }

            // 待機中にブックが変更された場合はそのブックで再待機
            if (_isBookChanged)
            {
                Wait(token);
            }

            void BookHub_IsBusyChanged(object? sender, JobIsBusyChangedEventArgs e)
            {
                if (!e.IsBusy)
                {
                    eventFlag.Set();
                }
            }

            void BookOperation_BookChanged(object? sender, BookChangedEventArgs e)
            {
                _isBookChanged = true;
                eventFlag.Set();
            }

            void BookControl_ViewContentsLoading(object? sender, ViewContentsLoadingEventArgs e)
            {
                if (!e.IsLoading)
                {
                    eventFlag.Set();
                }
            }
        }

        #endregion
    }
}
