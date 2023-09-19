using NeeView.Properties;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using NeeView.PageFrames;

namespace NeeView
{
    /// <summary>
    /// 現在ページに対する操作
    /// </summary>
    public class BookPageActionControl : IBookPageActionControl
    {
        private readonly Book _book;
        private readonly IBookControl _bookControl;
        private readonly PageFrameBox _box;

        public BookPageActionControl(PageFrameBox box, IBookControl bookControl)
        {
            _box = box;
            _book = _box.Book;
            _bookControl = bookControl;
        }

        #region ページ削除

        // 現在表示しているページのファイル削除可能？
        public bool CanDeleteFile()
        {
            var page = _book?.CurrentPage;
            if (page is null) return false;
            return CanDeleteFile(new List<Page>() { page });
        }

        // 現在表示しているページのファイルを削除する
        public async Task DeleteFileAsync()
        {
            var page = _book?.CurrentPage;
            if (page is null) return;
            await DeleteFileAsync(new List<Page>() { page });
        }

        // 指定ページのファル削除可能？
        public bool CanDeleteFile(List<Page> pages)
        {
            if (!pages.Any()) return false;

            return Config.Current.System.IsFileWriteAccessEnabled && PageFileIO.CanDeletePage(pages);
        }

        // 指定ページのファイルを削除する
        public async Task DeleteFileAsync(List<Page> pages)
        {
            var isCompletely = pages.Any(e => !e.ArchiveEntry.Archiver.IsFileSystem);
            if (Config.Current.System.IsRemoveConfirmed || isCompletely)
            {
                var dialog = await PageFileIO.CreateDeleteConfirmDialog(pages, isCompletely);
                if (!dialog.ShowDialog().IsPossible)
                {
                    return;
                }
            }

            try
            {
                await PageFileIO.DeletePageAsync(pages);
            }
            catch (Exception ex)
            {
                new MessageDialog($"{Resources.Word_Cause}: {ex.Message}", Resources.FileDeleteErrorDialog_Title).ShowDialog();
            }

            _bookControl.ReLoad();
        }

        #endregion ページ削除

        #region ページ出力

        // ファイルの場所を開くことが可能？
        public bool CanOpenFilePlace()
        {
            return _book?.CurrentPage != null;
        }

        // ファイルの場所を開く
        public void OpenFilePlace()
        {
            if (CanOpenFilePlace())
            {
                string? place = _book?.CurrentPage?.GetFolderOpenPlace();
                if (place != null)
                {
                    ExternalProcess.Start("explorer.exe", "/select,\"" + place + "\"");
                }
            }
        }


        // 外部アプリで開く
        public void OpenApplication(OpenExternalAppCommandParameter parameter)
        {
            var book = this._book;
            if (book is null) return;

            if (CanOpenFilePlace())
            {
                try
                {
                    var external = new ExternalAppUtility();
                    var pages = CollectPages(book, parameter.MultiPagePolicy);
                    external.Call(pages, parameter, CancellationToken.None);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    new MessageDialog(ex.Message, Properties.Resources.OpenApplicationErrorDialog_Title).ShowDialog();
                }
            }
        }

        private static List<Page> CollectPages(Book book, MultiPagePolicy policy)
        {
            if (book is null)
            {
                return new List<Page>();
            }

            var pages = book.CurrentPages.Distinct();

            switch (policy)
            {
                case MultiPagePolicy.Once:
                    pages = pages.Take(1);
                    break;

                case MultiPagePolicy.AllLeftToRight:
                    if (book.Setting.BookReadOrder == PageReadOrder.RightToLeft)
                    {
                        pages = pages.Reverse();
                    }
                    break;
            }

            return pages.ToList();
        }


        // クリップボードにコピー
        public void CopyToClipboard(CopyFileCommandParameter parameter)
        {
            var book = this._book;
            if (book is null) return;

            if (CanOpenFilePlace())
            {
                try
                {
                    var pages = CollectPages(book, parameter.MultiPagePolicy);
                    ClipboardUtility.Copy(pages, parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.Word_Cause}: {e.Message}", Resources.CopyErrorDialog_Title).ShowDialog();
                }
            }
        }

        /// <summary>
        /// ファイル保存可否
        /// </summary>
        /// <returns></returns>
        public bool CanExport()
        {
            return _box.GetSelectedPageFrameContent()?.ViewContents.FirstOrDefault() is IHasImageSource;
        }


        // ファイルに保存する (ダイアログ)
        // TODO: OutOfMemory対策
        // TODO: ダイアログにリソースを直接渡すようにする
        public void ExportDialog(ExportImageAsCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    var exportImageProceduralDialog = new ExportImageProceduralDialog();
                    exportImageProceduralDialog.Owner = MainViewComponent.Current.GetWindow();
                    exportImageProceduralDialog.Show(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.ImageExportErrorDialog_Message}\n{Resources.Word_Cause}: {e.Message}", Resources.ImageExportErrorDialog_Title).ShowDialog();
                    return;
                }
            }
        }

        // ファイルに保存する
        public void Export(ExportImageCommandParameter parameter)
        {
            if (CanExport())
            {
                try
                {
                    ExportImageProcedure.Execute(parameter);
                }
                catch (Exception e)
                {
                    new MessageDialog($"{Resources.ImageExportErrorDialog_Message}\n{Resources.Word_Cause}: {e.Message}", Resources.ImageExportErrorDialog_Title).ShowDialog();
                    return;
                }
            }
        }

#endregion ページ出力
    }
}