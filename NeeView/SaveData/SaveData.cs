using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace NeeView
{
    public class SaveData
    {
        static SaveData() => Current = new SaveData();
        public static SaveData Current { get; }

        private string? _pagemarkFilenameToDelete;
        private bool _historyMergeFlag;
        private DateTime _historyLastWriteTime;

        // 設定のバックアップを１起動に付き１回に制限するフラグ
        private bool _backupOnce = true;


        private SaveData()
        {
            App.Current.CriticalError += (s, e) => DisableSave();
        }

#if false
        public const string UserSettingFileName = "UserSetting.json";
        public const string HistoryFileName = "History.json";
        public const string BookmarkFileName = "Bookmark.json";
        public const string PagemarkFileName = "Pagemark.json";
        public const string CustomThemeFolder = "Themes";
        public const string PlaylistsFolder = "Playlists";
        public const string ScriptsFolder = "Scripts";

        public static string DefaultHistoryFilePath => Path.Combine(Environment.LocalApplicationDataPath, HistoryFileName);
        public static string DefaultBookmarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, BookmarkFileName);
        public static string DefaultPagemarkFilePath => Path.Combine(Environment.LocalApplicationDataPath, PagemarkFileName);
        public static string DefaultCustomThemeFolder => Environment.GetUserDataPath(CustomThemeFolder);
        public static string DefaultPlaylistsFolder => Environment.GetUserDataPath(PlaylistsFolder);
        public static string DefaultScriptsFolder => Environment.GetUserDataPath(ScriptsFolder);
#endif

        public static string UserSettingFilePath => App.Current.Option.SettingFilename ?? throw new InvalidOperationException("UserSettingFilePath must not be null");
        public static string HistoryFilePath => Config.Current.History.HistoryFilePath;
        public static string BookmarkFilePath => Config.Current.Bookmark.BookmarkFilePath;

        public bool IsEnableSave { get; private set; } = true;


        public void DisableSave()
        {
            IsEnableSave = false;
        }

        #region Load

        /// <summary>
        /// 設定ファイルの読み込み (bytes)
        /// </summary>
        /// <returns>設定ファイルのバイト列</returns>
        public byte[]? LoadUserSettingBytes(bool cancellable)
        {
            byte[]? bytes;
            using (ProcessLock.Lock())
            {
                var filename = App.Current.Option.SettingFilename;
                var extension = Path.GetExtension(filename)?.ToLowerInvariant();
                if (extension == ".json" && File.Exists(filename))
                {
                    var failedDialog = CreateUserSettingLoadFailedDialog(cancellable);
                    bytes = SafetyLoad(UserSettingTools.LoadBytes, filename, failedDialog, LoadUserSettingBackupCallback);
                }
                else
                {
                    bytes = null;
                }
            }
            return bytes;
        }

        /// <summary>
        /// 設定の読み込み (from bytes)
        /// </summary>
        /// <param name="bytes">設定ファイルのバイト列</param>
        /// <returns>UserSetting</returns>
        public UserSetting LoadUserSetting(byte[] bytes, bool cancellable)
        {
            try
            {
                using var stream = new MemoryStream(bytes);
                var setting = UserSettingTools.Load(stream);
                return setting ?? new UserSetting();
            }
            catch (Exception ex)
            {
                var failedDialog = CreateUserSettingLoadFailedDialog(cancellable);
                var result = AppDispatcher.Invoke(() => failedDialog.ShowDialog(ex));
                if (result != true)
                {
                    throw new OperationCanceledException();
                }

                return new UserSetting();
            }
        }

        /// <summary>
        /// 設定の読み込み (from UserSetting.json)
        /// </summary>
        /// <returns>UserSetting</returns>
        public UserSetting LoadUserSetting(bool cancellable)
        {
            if (App.Current.IsMainWindowLoaded)
            {
                Setting.SettingWindow.Current?.Cancel();
                MainWindowModel.Current.CloseCommandParameterDialog();
            }

            UserSetting? setting;

            using (ProcessLock.Lock())
            {
                var filename = App.Current.Option.SettingFilename;
                var extension = Path.GetExtension(filename)?.ToLowerInvariant();
                if (extension == ".json" && File.Exists(filename))
                {
                    var failedDialog = CreateUserSettingLoadFailedDialog(cancellable);
                    setting = SafetyLoad(UserSettingTools.Load, filename, failedDialog, LoadUserSettingBackupCallback);
                }
                else
                {
                    setting = null;
                }
            }

            return setting ?? new UserSetting();
        }

        /// <summary>
        /// UserSetting 用エラーダイアログ生成
        /// </summary>
        /// <param name="cancellable">キャンセルできる？</param>
        /// <returns></returns>
        private static LoadFailedDialog CreateUserSettingLoadFailedDialog(bool cancellable)
        {
            var failedDialog = new LoadFailedDialog("@Notice.LoadSettingFailed", "@Notice.LoadSettingFailedTitle");
            failedDialog.OKCommand = new UICommand("@Notice.LoadSettingFailedButtonContinue") { IsPossible = true };
            if (cancellable)
            {
                failedDialog.CancelCommand = new UICommand("@Notice.LoadSettingFailedButtonQuit") { Alignment = UICommandAlignment.Left };
            }
            return failedDialog;
        }

        private void LoadUserSettingBackupCallback()
        {
            _backupOnce = false;
        }


        // 履歴読み込み
        public void LoadHistory()
        {
            using (ProcessLock.Lock())
            {
                var filename = HistoryFilePath;
                var extension = Path.GetExtension(filename).ToLowerInvariant();
                var failedDialog = new LoadFailedDialog("@Notice.LoadHistoryFailed", "@Notice.LoadHistoryFailedTitle");

                var fileInfo = new FileInfo(filename);
                if (extension == ".json" && fileInfo.Exists)
                {
                    BookHistoryCollection.Memento? memento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, failedDialog);
                    BookHistoryCollection.Current.Restore(memento, true);
                    _historyLastWriteTime = fileInfo.LastWriteTime;
                }
            }
        }

        // ブックマーク読み込み
        public void LoadBookmark()
        {
            using (ProcessLock.Lock())
            {
                var filename = BookmarkFilePath;
                var extension = Path.GetExtension(filename).ToLowerInvariant();
                var failedDialog = new LoadFailedDialog("@Notice.LoadBookmarkFailed", "@Notice.LoadBookmarkFailedTitle");

                if (extension == ".json" && File.Exists(filename))
                {
                    BookmarkCollection.Memento? memento = SafetyLoad(BookmarkCollection.Memento.Load, filename, failedDialog);
                    BookmarkCollection.Current.Restore(memento);
                }
            }
        }

        /// <summary>
        /// 正規ファイルの読み込みに失敗したらバックアップからの復元を試みる。
        /// エラー時にはダイアログ表示。選択によってはOperationCancelExceptionを発生させる。
        /// </summary>
        private static T? SafetyLoad<T>(Func<string, T?> load, string path, LoadFailedDialog loadFailedDialog, Action? loadBackupCallback = null)
            where T : class
        {
            try
            {
                return SafetyLoad(load, path, loadBackupCallback);
            }
            catch (Exception ex)
            {
                if (loadFailedDialog != null)
                {
                    var result = AppDispatcher.Invoke(() => loadFailedDialog.ShowDialog(ex));
                    if (result != true)
                    {
                        throw new OperationCanceledException();
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// 正規ファイルの読み込みに失敗したらバックアップからの復元を試みる
        /// </summary>
        private static T? SafetyLoad<T>(Func<string, T?> load, string path, Action? loadBackupCallback)
            where T : class
        {
            var old = path + ".bak";

            if (File.Exists(path))
            {
                try
                {
                    return load(path);
                }
                catch
                {
                    if (File.Exists(old))
                    {
                        loadBackupCallback?.Invoke();
                        return load(old);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else if (File.Exists(old))
            {
                return load(old);
            }
            else
            {
                return null;
            }
        }

        #endregion Load

        #region Save

        /// <summary>
        /// 設定の保存
        /// </summary>
        public void SaveUserSetting()
        {
            if (!IsEnableSave) return;

            using (ProcessLock.Lock())
            {
                bool createBackup = Config.Current.System.IsSettingBackup && _backupOnce;
                SafetySave(UserSettingTools.Save, UserSettingFilePath, createBackup);
                _backupOnce = false;
            }
        }

        /// <summary>
        /// 古いファイルを削除
        /// </summary>
        private static void DeleteLegacyFile(string filename)
        {
            using (ProcessLock.Lock())
            {
                Debug.WriteLine($"Remove: {filename}");
                FileIO.DeleteFile(filename);

                // バックアップファイルも削除
                var backup = filename + ".old";
                if (File.Exists(backup))
                {
                    Debug.WriteLine($"Remove: {backup}");
                    FileIO.DeleteFile(backup);
                }
            }
        }

        /// <summary>
        /// 履歴をファイルに保存
        /// </summary>
        public void SaveHistory()
        {
            if (!IsEnableSave) return;
            if (!Config.Current.History.IsSaveHistory) return;

            using (ProcessLock.Lock())
            {
                //Debug.WriteLine("SaveData.SaveHistory(): saving.");
                var bookHistoryMemento = BookHistoryCollection.Current.CreateMemento();

                try
                {
                    // NOTE: 一度マージが発生したらその後は常にマージを行う。負荷が高いのが問題。
                    var fileInfo = new FileInfo(HistoryFilePath);
                    if (fileInfo.Exists && (_historyMergeFlag || fileInfo.LastWriteTime > _historyLastWriteTime))
                    {
                        //Debug.WriteLine("SaveData.SaveHistory(): merge.");
                        var failedDialog = new LoadFailedDialog("@Notice.LoadHistoryFailed", "@Notice.LoadHistoryFailedTitle");
                        var margeMemento = SafetyLoad(BookHistoryCollection.Memento.Load, HistoryFilePath, failedDialog);
                        bookHistoryMemento.Merge(margeMemento);
                        _historyMergeFlag = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                SafetySave(bookHistoryMemento.Save, HistoryFilePath, false);
                _historyLastWriteTime = File.GetLastWriteTime(HistoryFilePath);
            }
        }

        /// <summary>
        /// 必要であるならば、Historyを削除
        /// </summary>
        public void DeleteHistoryIfNotSave()
        {
            if (!IsEnableSave) return;
            if (Config.Current.History.IsSaveHistory) return;

            using (ProcessLock.Lock())
            {
                FileIO.DeleteFile(HistoryFilePath);
            }
        }

        /// <summary>
        /// Bookmarkの保存
        /// </summary>
        public void SaveBookmark()
        {
            if (!IsEnableSave) return;
            if (!Config.Current.Bookmark.IsSaveBookmark) return;

            using (ProcessLock.Lock())
            {
                var bookmarkMemento = BookmarkCollection.Current.CreateMemento();
                SafetySave(bookmarkMemento.Save, BookmarkFilePath, false);
            }
        }

        /// <summary>
        /// 必要であるならば、Bookmarkを削除
        /// </summary>
        public void DeleteBookmarkIfNotSave()
        {
            if (!IsEnableSave) return;
            if (Config.Current.Bookmark.IsSaveBookmark) return;

            using (ProcessLock.Lock())
            {
                FileIO.DeleteFile(BookmarkFilePath);
            }
        }

        /// <summary>
        /// 必要であるならば、古い設定ファイルを削除
        /// </summary>
        public void DeleteLegacyPagemark()
        {
            if (_pagemarkFilenameToDelete == null) return;

            DeleteLegacyFile(_pagemarkFilenameToDelete);
            _pagemarkFilenameToDelete = null;
        }

        /// <summary>
        /// アプリ強制終了でもファイルがなるべく破壊されないような保存
        /// </summary>
        /// <param name="save">SAVE関数</param>
        /// <param name="path">保存ファイル名</param>
        /// <param name="createBackup">バックアップを作る</param>
        private static void SafetySave(Action<string> save, string path, bool createBackup)
        {
            if (save is null) throw new ArgumentNullException(nameof(save));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var newFileName = path + ".new.tmp";
            var backupFileName = createBackup ? path + ".bak" : null;

            lock (App.Current.Lock)
            {
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(newFileName);
                        save(newFileName);
                        File.Replace(newFileName, path, backupFileName);
                    }
                    catch
                    {
                        File.Delete(newFileName);
                        throw;
                    }
                }
                else
                {
                    save(path);
                }
            }
        }

        #endregion Save
    }


    /// <summary>
    /// データロードエラーダイアログ
    /// </summary>
    public class LoadFailedDialog
    {
        public LoadFailedDialog(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public string Title { get; set; }
        public string Message { get; set; }
        public UICommand OKCommand { get; set; } = UICommands.OK;
        public UICommand? CancelCommand { get; set; }


        public bool ShowDialog(Exception ex)
        {
            var textBox = new System.Windows.Controls.TextBox()
            {
                IsReadOnly = true,
                Text = ResourceService.GetString(Message) + System.Environment.NewLine + ex.Message,
                TextWrapping = System.Windows.TextWrapping.Wrap,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled,
            };

            var dialog = new MessageDialog(textBox, ResourceService.GetString(Title));
            dialog.SizeToContent = System.Windows.SizeToContent.Manual;
            dialog.Height = 320.0;
            dialog.ResizeMode = System.Windows.ResizeMode.CanResize;
            dialog.Commands.Add(OKCommand);
            if (CancelCommand != null)
            {
                dialog.Commands.Add(CancelCommand);
            }

            var result = dialog.ShowDialog();
            return result.Command == OKCommand || CancelCommand == null;
        }
    }
}
