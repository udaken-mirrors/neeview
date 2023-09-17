using NeeLaboratory.ComponentModel;
using NeeView.PageFrames;
using NeeView.Properties;
using System;
using System.Threading;

namespace NeeView
{

    public class BookPageTerminator : IDisposable
    {
        private readonly PageFrameBox _box;
        private readonly Book _book;
        private int _pageTerminating;
        private readonly IBookPageControl _control;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();


        public BookPageTerminator(PageFrameBox box, IBookPageControl control)
        {
            _box = box;
            _book = _box.Book;
            _control = control;

            _disposables.Add(_box.SubscribePageTerminated(
                (s, e) => AppDispatcher.Invoke(() => Box_PageTerminated(s, e))));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        // ページ終端を超えて移動しようとするときの処理
        private void Box_PageTerminated(object? sender, PageTerminatedEventArgs e)
        {
            if (_pageTerminating > 0) return;

            // TODO ここでSlideShowを参照しているが、引数で渡すべきでは？
            if (SlideShow.Current.IsPlayingSlideShow && Config.Current.SlideShow.IsSlideShowByLoop)
            {
                _control.MoveToFirst(sender);
            }
            else
            {
                switch (Config.Current.Book.PageEndAction)
                {
                    case PageEndAction.Loop:
                        PageEndAction_Loop(sender, e);
                        break;

                    case PageEndAction.NextBook:
                        PageEndAction_NextBook(sender, e);
                        break;

                    case PageEndAction.Dialog:
                        PageEndAction_Dialog(sender, e);
                        break;

                    default:
                        PageEndAction_None(sender, e, true);
                        break;
                }
            }
        }

        private void PageEndAction_Loop(object? sender, PageTerminatedEventArgs e)
        {
            if (e.Direction < 0)
            {
                _control.MoveToLast(sender);
            }
            else
            {
                _control.MoveToFirst(sender);
            }
            if (Config.Current.Book.IsNotifyPageLoop)
            {
                InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_BookOperationPageLoop);
            }
        }

        private void PageEndAction_NextBook(object? sender, PageTerminatedEventArgs e)
        {
            AppDispatcher.Invoke(async () =>
            {
                if (e.Direction < 0)
                {
                    await BookshelfFolderList.Current.PrevFolder(BookLoadOption.LastPage);
                }
                else
                {
                    await BookshelfFolderList.Current.NextFolder(BookLoadOption.FirstPage);
                }
            });
        }

        private void PageEndAction_None(object? sender, PageTerminatedEventArgs e, bool notify)
        {
            if (SlideShow.Current.IsPlayingSlideShow)
            {
                // スライドショー解除
                SlideShow.Current.Stop();
            }

            // 通知。本の場合のみ処理。メディアでは不要
            else if (notify && this._book != null && !this._book.IsMedia)
            {
                if (e.Direction < 0)
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_FirstPage);
                }
                else
                {
                    SoundPlayerService.Current.PlaySeCannotMove();
                    InfoMessage.Current.SetMessage(InfoMessageType.Notify, Properties.Resources.Notice_LastPage);
                }
            }
        }

        private void PageEndAction_Dialog(object? sender, PageTerminatedEventArgs e)
        {
            Interlocked.Increment(ref _pageTerminating);

            AppDispatcher.BeginInvoke(() =>
            {
                try
                {
                    PageEndAction_DialogCore(sender, e);
                }
                finally
                {
                    Interlocked.Decrement(ref _pageTerminating);
                }
            });
        }

        private void PageEndAction_DialogCore(object? sender, PageTerminatedEventArgs e)
        {
            var title = (e.Direction < 0) ? Resources.Notice_FirstPage : Resources.Notice_LastPage;
            var dialog = new MessageDialog(Resources.PageEndDialog_Message, title);
            var nextCommand = new UICommand(Properties.Resources.PageEndAction_NextBook);
            var loopCommand = new UICommand(Properties.Resources.PageEndAction_Loop);
            var noneCommand = new UICommand(Properties.Resources.PageEndAction_None);
            dialog.Commands.Add(nextCommand);
            dialog.Commands.Add(loopCommand);
            dialog.Commands.Add(noneCommand);
            var result = dialog.ShowDialog(App.Current.MainWindow);

            if (result.Command == nextCommand)
            {
                PageEndAction_NextBook(sender, e);
            }
            else if (result.Command == loopCommand)
            {
                PageEndAction_Loop(sender, e);
            }
            else
            {
                PageEndAction_None(sender, e, false);
            }
        }

    }

}
