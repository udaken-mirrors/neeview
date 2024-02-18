using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class SystemConfig : BindableBase
    {
        private ArchiveEntryCollectionMode _archiveRecursiveMode = ArchiveEntryCollectionMode.IncludeSubArchives;
        private bool _isNetworkEnabled = true;
        private bool _isSettingBackup;
        private bool _isHiddenFileVisible;
        private bool _isFileWriteAccessEnabled = false;
        private string? _language;
        private BookPageCollectMode _bookPageCollectMode = BookPageCollectMode.ImageAndBook;
        private bool _isRemoveConfirmed = true;
        private bool _isRemoveWantNukeWarning;
        private bool _isSyncUserSetting = true;
        private bool _isIgnoreImageDpi = true;
        private string _downloadPath = "";
        private bool _isOpenBookAtCurrentPlace;
        private bool _isNaturalSortEnabled;
        private bool _isInputMethodEnabled;
        private DestinationFolderCollection _destinationFolderCollection = new();
        private ExternalAppCollection _externalAppCollection = new() { new ExternalApp() };
        private string? _textEditor;
        private string? _webBrowser;
        private bool _isIncrementalSearchEnabled = true;
        private int _searchHistorySize = 8;

        [JsonInclude, JsonPropertyName(nameof(DateTimeFormat))]
        public string? _dateTimeFormat;

        [JsonInclude, JsonPropertyName(nameof(TemporaryDirectory))]
        public string? _temporaryDirectory;


        /// <summary>
        /// 言語
        /// </summary>
        [PropertyStrings]
        public string Language
        {
            get { return _language ?? (_language = Properties.TextResources.LanguageResource.ValidateCultureInfo(CultureInfo.CurrentCulture).Name); }
            set { SetProperty(ref _language, value); }
        }

        /// <summary>
        /// 日付フォーマット
        /// </summary>
        [JsonIgnore]
        [PropertyMember]
        [NotNull]
        public string? DateTimeFormat
        {
            get { return _dateTimeFormat ?? DateTimeTools.DefaultDateTimePattern; }
            set { SetProperty(ref _dateTimeFormat, (string.IsNullOrWhiteSpace(value) || value == DateTimeTools.DefaultDateTimePattern) ? null : value); }
        }

        [PropertyMember]
        public ArchiveEntryCollectionMode ArchiveRecursiveMode
        {
            get { return _archiveRecursiveMode; }
            set { SetProperty(ref _archiveRecursiveMode, value); }
        }

        // ページ収集モード
        [PropertyMember]
        public BookPageCollectMode BookPageCollectMode
        {
            get { return _bookPageCollectMode; }
            set { SetProperty(ref _bookPageCollectMode, value); }
        }

        [PropertyMember]
        public bool IsRemoveConfirmed
        {
            get { return _isRemoveConfirmed; }
            set { SetProperty(ref _isRemoveConfirmed, value); }
        }

        [PropertyMember]
        public bool IsRemoveWantNukeWarning
        {
            get { return _isRemoveWantNukeWarning; }
            set { SetProperty(ref _isRemoveWantNukeWarning, value); }
        }

        // ネットワークアクセス許可
        [PropertyMember]
        public bool IsNetworkEnabled
        {
            get { return _isNetworkEnabled || Environment.IsAppxPackage; } // Appxは強制ON
            set { SetProperty(ref _isNetworkEnabled, value); }
        }

        // 設定データの同期
        [PropertyMember]
        public bool IsSyncUserSetting
        {
            get { return _isSyncUserSetting; }
            set { SetProperty(ref _isSyncUserSetting, value); }
        }

        // 設定データのバックアップ作成
        [PropertyMember]
        public bool IsSettingBackup
        {
            get { return _isSettingBackup || Environment.IsAppxPackage; }  // Appxは強制ON
            set { SetProperty(ref _isSettingBackup, value); }
        }

        // 画像のDPI非対応
        [PropertyMember]
        public bool IsIgnoreImageDpi
        {
            get { return _isIgnoreImageDpi; }
            set { SetProperty(ref _isIgnoreImageDpi, value); }
        }

        // テンポラリフォルダーの場所
        [JsonIgnore]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string TemporaryDirectory
        {
            get { return _temporaryDirectory ?? Temporary.TempRootPathDefault; }
            set { SetProperty(ref _temporaryDirectory, (string.IsNullOrWhiteSpace(value) || value.Trim() == Temporary.TempRootPathDefault) ? null : value.Trim()); }
        }

        // ダウンロードファイル置き場
        [DefaultValue("")]
        [PropertyPath(FileDialogType = FileDialogType.Directory)]
        public string DownloadPath
        {
            get { return _downloadPath; }
            set { SetProperty(ref _downloadPath, value ?? ""); }
        }

        // 隠しファイルを表示する？
        [PropertyMember]
        public bool IsHiddenFileVisible
        {
            get { return _isHiddenFileVisible; }
            set { SetProperty(ref _isHiddenFileVisible, value); }
        }

        [PropertyMember]
        public bool IsFileWriteAccessEnabled
        {
            get { return _isFileWriteAccessEnabled; }
            set { SetProperty(ref _isFileWriteAccessEnabled, value); }
        }


        // 「ブックを開く」ダイアログを現在の場所を基準にして開く
        // TODO: LoadAs のコマンドパラメータにする
        [PropertyMember]
        public bool IsOpenBookAtCurrentPlace
        {
            get { return _isOpenBookAtCurrentPlace; }
            set { SetProperty(ref _isOpenBookAtCurrentPlace, value); }
        }

        // カスタム自然順ソート
        [PropertyMember]
        public bool IsNaturalSortEnabled
        {
            get { return _isNaturalSortEnabled; }
            set { SetProperty(ref _isNaturalSortEnabled, value); }
        }

        // テキストボックス以外でのIME有効 (現状では非公開)
        [PropertyMember]
        public bool IsInputMethodEnabled
        {
            get { return _isInputMethodEnabled; }
            set { SetProperty(ref _isInputMethodEnabled, value); }
        }


        // コピーまたは移動先フォルダーのリスト
        [PropertyMember]
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public DestinationFolderCollection DestinationFolderCollection
        {
            get { return _destinationFolderCollection; }
            set { SetProperty(ref _destinationFolderCollection, value); }
        }

        // 外部実行アプリ設定のリスト
        [PropertyMember]
        [PropertyMapIgnore]
        [ObjectMergeReferenceCopy]
        public ExternalAppCollection ExternalAppCollection
        {
            get { return _externalAppCollection; }
            set { SetProperty(ref _externalAppCollection, value); }
        }

        // テキストエディター
        [PropertyPath(Filter = "EXE|*.exe|All|*.*")]
        public string? TextEditor
        {
            get { return _textEditor; }
            set { SetProperty(ref _textEditor, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        // ウェブブラウザー
        [PropertyPath(Filter = "EXE|*.exe|All|*.*")]
        public string? WebBrowser
        {
            get { return _webBrowser; }
            set { SetProperty(ref _webBrowser, string.IsNullOrWhiteSpace(value) ? null : value.Trim()); }
        }

        /// <summary>
        /// インクリメンタルサーチ有効
        /// </summary>
        [PropertyMember]
        public bool IsIncrementalSearchEnabled
        {
            get { return _isIncrementalSearchEnabled; }
            set { SetProperty(ref _isIncrementalSearchEnabled, value); }
        }

        /// <summary>
        /// 検索ボックスの履歴数
        /// </summary>
        [PropertyMember]
        public int SearchHistorySize
        {
            get { return _searchHistorySize; }
            set { SetProperty(ref _searchHistorySize, Math.Max(0, value)); }
        }

        #region Obsolete

        [Obsolete("Typo"), Alternative(nameof(IsHiddenFileVisible), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        public bool IsHiddenFileVisibled
        {
            get { return IsHiddenFileVisible; }
            set { IsHiddenFileVisible = value; }
        }

        [Obsolete("Typo"), Alternative(nameof(IsOpenBookAtCurrentPlace), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        public bool IsOpenbookAtCurrentPlace
        {
            get { return IsOpenBookAtCurrentPlace; }
            set { IsOpenBookAtCurrentPlace = value; }
        }

        [Obsolete("Typo"), Alternative(nameof(DestinationFolderCollection), 41, ScriptErrorLevel.Info)] // ver.41
        [JsonIgnore]
        public DestinationFolderCollection DestinationFodlerCollection
        {
            get { return DestinationFolderCollection; }
            set { DestinationFolderCollection = value; }
        }

        #endregion Obsolete
    }
}
