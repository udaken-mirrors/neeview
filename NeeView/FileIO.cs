﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// File I/O
    /// </summary>
    public class FileIO
    {
        public static FileIO Current { get; } = new FileIO();

        //
        public FileIO()
        {
        }

        // ファイル削除
        public static void RemoveFile(string filename)
        {
            new FileInfo(filename).Delete();
        }


        /// <summary>
        /// クリップボードにコピー
        /// </summary>
        /// <param name="info"></param>
        public void CopyToClipboard(FolderItem info)
        {
            if (info.IsEmpty) return;

            var files = new List<string>();
            files.Add(info.Path);
            var data = new DataObject();
            data.SetData(DataFormats.FileDrop, files.ToArray());
            data.SetData(DataFormats.UnicodeText, string.Join("\r\n", files));
            Clipboard.SetDataObject(data);
        }


        #region Remove

        // ページ削除可能？
        public bool CanRemovePage(Page page)
        {
            if (page == null) return false;
            if (!page.Entry.IsFileSystem) return false;

            var path = page.GetFilePlace();
            return (File.Exists(path) || Directory.Exists(path));
        }

        // ページを削除する
        public async Task RemovePageAsync(Page page)
        {
            if (page == null) return;

            bool isRemoved = await RemoveAsync(page.GetFilePlace(), "ページを削除します", async () => await new PageVisual(page).CreateVisualContentAsync(new System.Windows.Size(64, 64), true));

            var book = BookHub.Current.Book;

            // ページを本から削除
            if (isRemoved == true && book != null)
            {
                book.RequestRemove(this, page);
            }
        }

        // ファイルを削除
        public async Task<bool> RemoveAsync(string path, string title = null, Func<Task<FrameworkElement>> createThumbnailAsync = null)
        {
            if (path == null) return false;

            bool isFile = System.IO.File.Exists(path);
            bool isDirectory = System.IO.Directory.Exists(path);
            if (!isFile && !isDirectory) return false;

            if (FileIOProfile.Current.IsRemoveConfirmed)
            {
                string typeName = isDirectory ? "フォルダー" : "ファイル";

                var dockPanel = new DockPanel();

                var message = new TextBlock();
                message.Text = $"この{typeName}をごみ箱に移動しますか？";
                message.Margin = new Thickness(0, 0, 0, 10);
                DockPanel.SetDock(message, Dock.Top);
                dockPanel.Children.Add(message);

                FrameworkElement thumbnail = null;
                if (createThumbnailAsync != null)
                {
                    thumbnail = await createThumbnailAsync();
                }
                if (thumbnail == null)
                {
                    thumbnail = new Image
                    {
                        SnapsToDevicePixels = true,
                        Source = NeeLaboratory.IO.FileSystem.GetTypeIconSource(path, NeeLaboratory.IO.FileSystem.IconSize.Normal),
                        Width = 32,
                        Height = 32
                    };
                }

                if (thumbnail != null)
                {
                    thumbnail.Margin = new Thickness(0, 0, 10, 0);
                    dockPanel.Children.Add(thumbnail);
                }

                var textblock = new TextBlock();
                textblock.Text = path;
                textblock.VerticalAlignment = VerticalAlignment.Bottom;
                textblock.TextWrapping = TextWrapping.Wrap;
                textblock.Margin = new Thickness(0, 0, 0, 2);
                dockPanel.Children.Add(textblock);

                //
                var dialog = new MessageDialog(dockPanel, title ?? $"{typeName}を削除します");
                dialog.Commands.Add(UICommands.Remove);
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();

                if (answer != UICommands.Remove) return false;
            }

            return await RemoveAsyncInner(path);
        }

        // ファイルを削除 コア
        private async Task<bool> RemoveAsyncInner(string path)
        {
            var _bookHub = BookHub.Current;
            int retryCount = 1;

            Retry:

            try
            {
                // 開いている本であるならば閉じる
                if (_bookHub.Address == path)
                {
                    await _bookHub.RequestUnload(true).WaitAsync();
                }

                // ゴミ箱に捨てる
                bool isDirectory = System.IO.Directory.Exists(path);
                if (isDirectory)
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }
                else
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(path, Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                }

                //
                return true;
            }
            catch (Exception ex)
            {
                if (retryCount > 0)
                {
                    await Task.Delay(1000);
                    retryCount--;
                    goto Retry;
                }
                else
                {
                    var dialog = new MessageDialog($"削除できませんでした。もう一度実行しますか？\n\n原因: {ex.Message}", "削除できませんでした。リトライしますか？");
                    dialog.Commands.Add(UICommands.Retry);
                    dialog.Commands.Add(UICommands.Cancel);
                    var confirm = dialog.ShowDialog();
                    if (confirm == UICommands.Retry)
                    {
                        retryCount = 1;
                        goto Retry;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        #endregion


        #region Rename

        // ファイル名前変更
        public async Task<string> RenameAsync(FolderItem file, string newName)
        {
            newName = newName?.Trim().TrimEnd(' ', '.');

            // ファイル名に使用できない
            if (string.IsNullOrWhiteSpace(newName))
            {
                var dialog = new MessageDialog($"指定されたファイル名は無効です。", "名前を変更できません");
                dialog.ShowDialog();
                return null;
            }

            //ファイル名に使用できない文字
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            int invalidCharsIndex = newName.IndexOfAny(invalidChars);
            if (invalidCharsIndex >= 0)
            {
                var invalids = string.Join(" ", newName.Where(e => invalidChars.Contains(e)).Distinct());

                var dialog = new MessageDialog($"ファイル名に使用できない文字が含まれています。\n\n{invalids}", "名前を変更できません");
                dialog.ShowDialog();

                return null;
            }

            // ファイル名に使用できない
            var match = new Regex(@"^(CON|PRN|AUX|NUL|COM[0-9]|LPT[0-9])(\.|$)", RegexOptions.IgnoreCase).Match(newName);
            if (match.Success)
            {
                var dialog = new MessageDialog($"指定されたデバイス名は無効です。\n\n{match.Groups[1].Value.ToUpper()}", "名前を変更できません");
                dialog.ShowDialog();
                return null;
            }

            string src = file.Path;
            string dst = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(src), newName);

            // 全く同じ名前なら処理不要
            if (src == dst) return null;

            // 拡張子変更確認
            if (!file.IsDirectory)
            {
                var srcExt = System.IO.Path.GetExtension(src);
                var dstExt = System.IO.Path.GetExtension(dst);
                if (string.Compare(srcExt, dstExt, true) != 0)
                {
                    var dialog = new MessageDialog($"拡張子を変更すると、使えなくなる可能性があります。\nよろしいですか？", "拡張子を変更します");
                    dialog.Commands.Add(UICommands.Yes);
                    dialog.Commands.Add(UICommands.No);
                    var answer = dialog.ShowDialog();
                    if (answer != UICommands.Yes)
                    {
                        return null;
                    }
                }
            }

            // 大文字小文字の変換は正常
            if (string.Compare(src, dst, true) == 0)
            {
                // nop.
            }

            // 重複ファイル名回避
            else if (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst))
            {
                string dstBase = dst;
                string dir = System.IO.Path.GetDirectoryName(dst);
                string name = System.IO.Path.GetFileNameWithoutExtension(dst);
                string ext = System.IO.Path.GetExtension(dst);
                int count = 1;

                do
                {
                    dst = $"{dir}\\{name} ({++count}){ext}";
                }
                while (System.IO.File.Exists(dst) || System.IO.Directory.Exists(dst));

                // 確認
                var dialog = new MessageDialog($"{System.IO.Path.GetFileName(dstBase)} は既に存在しています。\n{System.IO.Path.GetFileName(dst)} に名前を変更しますか？", "同じ名前のファイルが存在しています");
                dialog.Commands.Add(new UICommand("名前を変える"));
                dialog.Commands.Add(UICommands.Cancel);
                var answer = dialog.ShowDialog();
                if (answer != dialog.Commands[0])
                {
                    return null;
                }
            }

            // 名前変更実行
            var result = await RenameAsyncInner(src, dst);

            return result ? dst : null;
        }

        // ファイル名前変更 コア
        private async Task<bool> RenameAsyncInner(string src, string dst)
        {
            var _bookHub = BookHub.Current;
            int retryCount = 1;

            Retry:

            try
            {
                bool isContinue = false;
                int requestLoadCount = _bookHub.RequestLoadCount;

                // 開いている本であるならば閉じる
                if (_bookHub.Address == src)
                {
                    isContinue = true;
                    await _bookHub.RequestUnload(false).WaitAsync();
                }

                // rename
                if (System.IO.Directory.Exists(src))
                {
                    System.IO.Directory.Move(src, dst);
                }
                else
                {
                    System.IO.File.Move(src, dst);
                }

                // 閉じた本を開き直す
                if (isContinue && requestLoadCount == _bookHub.RequestLoadCount)
                {
                    RenameHistory(src, dst);
                    _bookHub.RequestLoad(dst, null, BookLoadOption.Resume | BookLoadOption.IsBook, false);
                }

                return true;
            }
            catch (Exception ex)
            {
                if (retryCount > 0)
                {
                    await Task.Delay(1000);
                    retryCount--;
                    goto Retry;
                }

                var confirm = new MessageDialog($"名前の変更に失敗しました。もう一度実行しますか？\n\n{ex.Message}", "名前を変更できませんでした");
                confirm.Commands.Add(UICommands.Retry);
                confirm.Commands.Add(UICommands.Cancel);
                var answer = confirm.ShowDialog();
                if (answer == UICommands.Retry)
                {
                    retryCount = 1;
                    goto Retry;
                }
                else
                {
                    return false;
                }
            }
        }

        // 履歴上のファイル名変更
        private void RenameHistory(string src, string dst)
        {
            BookMementoCollection.Current.Rename(src, dst);
            PagemarkCollection.Current.Rename(src, dst);
        }

        #endregion

    }
}
