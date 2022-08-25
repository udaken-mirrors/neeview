using System;

// TODO: コマンド類の何時でも受付。ロード中だから弾く、ではない別の方法を。

namespace NeeView
{
    /// <summary>
    /// フォルダーリスト更新イベントパラメーター
    /// </summary>
    public class FolderListSyncEventArgs : EventArgs
    {
        public FolderListSyncEventArgs(string path, string parent, bool isKeepPlace)
        {
            Path = path;
            Parent = parent;
            this.isKeepPlace = isKeepPlace;
        }

        /// <summary>
        /// フォルダーリストで選択されて欲しい項目のパス
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// フォルダーリストの場所。アーカイブパス用。
        /// nullの場合Pathから求められる。
        /// </summary>
        public string Parent { get; set; }

        /// <summary>
        /// なるべくリストの選択項目を変更しないようにする
        /// </summary>
        public bool isKeepPlace { get; set; }
    }
}

