using System;
using System.Diagnostics;
using System.IO;
using NeeLaboratory.IO;
using NeeView.Data;
using NeeView.Threading;

namespace NeeView
{
    /// <summary>
    /// ブックマーク、ページマークは変更のたびに保存。
    /// 他プロセスからの要求でリロードを行う。
    /// </summary>
    public class SaveDataSync : IDisposable
    {
        // Note: Initialize()必須
        static SaveDataSync() => Current = new SaveDataSync();
        public static SaveDataSync Current { get; }


        private readonly DelayAction _delaySaveBookmark;


        private SaveDataSync()
        {
            _delaySaveBookmark = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.2), () => SaveBookmark(true, true), TimeSpan.FromSeconds(0.5));

            RemoteCommandService.Current.AddReciever("LoadUserSetting", LoadUserSetting);
            RemoteCommandService.Current.AddReciever("LoadHistory", LoadHistory);
            RemoteCommandService.Current.AddReciever("LoadBookmark", LoadBookmark);
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    BookmarkCollection.Current.BookmarkChanged -= BookmarkCollection_BookmarkChanged;
                    _delaySaveBookmark.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        public void Initialize()
        {
            BookmarkCollection.Current.BookmarkChanged += BookmarkCollection_BookmarkChanged;
        }

        private void BookmarkCollection_BookmarkChanged(object? sender, BookmarkCollectionChangedEventArgs e)
        {
            if (e.Action == EntryCollectionChangedAction.Reset) return;
            _delaySaveBookmark.Request();
        }

        public void Flush()
        {
            _delaySaveBookmark.Flush();
        }

        private void LoadUserSetting(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveData.UserSettingFileName} is updated by other process.");
            var setting = SaveData.Current.LoadUserSetting(false);
            UserSettingTools.Restore(setting);
        }

        private void LoadHistory(RemoteCommand command)
        {
            throw new NotImplementedException();
            // TODO: フラグ管理のみ？
        }

        private void LoadBookmark(RemoteCommand command)
        {
            Debug.WriteLine($"{SaveData.BookmarkFileName} is updated by other process.");
            SaveData.Current.LoadBookmark();
        }

        public void SaveUserSetting(bool sync, bool handleException)
        {
            Debug.WriteLine($"Save UserSetting");

            try
            {
                SaveData.Current.SaveUserSetting();
            }
            catch (Exception ex)
            {
                var message = Properties.Resources.FailedToSaveDataDialog_Setting_Message + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.Resources.FailedToSaveDataDialog_Title, ToastIcon.Error));
                    return;
                }
                else
                {
                    throw new IOException(message, ex);
                }
            }

            // TODO: 動作検証用に古い形式のデータも保存する
            ////SaveData.Current.SaveUserSettingV1();

            if (sync)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("LoadUserSetting"), RemoteCommandDelivery.All);
            }
        }

        private static void SaveHistory(bool handleException)
        {
            Debug.WriteLine($"Save History");

            try
            {
                SaveData.Current.SaveHistory();
            }
            catch (Exception ex)
            {
                var message = Properties.Resources.FailedToSaveDataDialog_History_Message + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.Resources.FailedToSaveDataDialog_Title, ToastIcon.Error));
                    return;
                }
                else
                {
                    throw new IOException(message, ex);
                }
            }
        }

        public void SaveBookmark(bool sync, bool handleException)
        {
            Debug.WriteLine($"Save Bookmark");

            try
            {
                SaveData.Current.SaveBookmark();
            }
            catch (Exception ex)
            {
                var message = Properties.Resources.FailedToSaveDataDialog_Bookmark_Message + System.Environment.NewLine + ex.Message;
                if (handleException)
                {
                    ToastService.Current.Show(new Toast(message, Properties.Resources.FailedToSaveDataDialog_Title, ToastIcon.Error));
                    return;
                }
                else
                {
                    throw new IOException(message, ex);
                }
            }

            if (sync)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("LoadBookmark"), RemoteCommandDelivery.All);
            }
        }

        private static void RemoveBookmarkIfNotSave()
        {
            SaveData.Current.RemoveBookmarkIfNotSave();
        }

        /// <summary>
        /// すべてのセーブ処理を行う
        /// </summary>
        public void SaveAll(bool sync, bool handleException)
        {
            Flush();
            SaveUserSetting(sync, handleException);
            SaveHistory(handleException);
            RemoveBookmarkIfNotSave();

            PlaylistHub.Current.Flush();
        }
    }
}
