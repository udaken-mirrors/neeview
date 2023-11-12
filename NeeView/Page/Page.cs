﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using NeeLaboratory.IO.Search;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class Page : IDisposable, INotifyPropertyChanged, IPageContentLoader, IPageThumbnailLoader, IHasPage, IRenameable, ISearchItem
    {
        private int _index;
        private readonly PageContent _content;
        private bool _isVisible;
        private bool _isMarked;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();


        public Page(string bookPrefix, PageContent content)
        {
            BookPrefix = bookPrefix;

            _content = content;
            _content.ContentChanged += Content_ContentChanged;
            _content.SizeChanged += Content_SizeChanged;

            _thumbnailSource = PageThumbnailFactory.Create(_content);
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? ContentChanged;

        [Subscribable]
        public event EventHandler? SizeChanged;



        public bool IsLoaded => _content.IsLoaded;

        public ArchiveEntry ArchiveEntry => _content.ArchiveEntry;

        public PageContent Content => _content;


        // 登録番号
        public int EntryIndex { get; set; }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                _content.Index = value;
            }
        }

        // TODO: 表示番号と内部番号のずれ
        public int IndexPlusOne => Index + 1;

        // ページ名 : エントリ名
        public string EntryName => ArchiveEntry.EntryFullName[BookPrefix.Length..];

        // ページ名：ファイル名のみ
        public string EntryLastName => ArchiveEntry.EntryLastName;

        // ページ名：スマートパス
        public string EntrySmartName => Prefix == null ? EntryName : EntryName[Prefix.Length..];

        // ページ名：フルパス名 (リンクはそのまま)
        public string EntryFullName => ArchiveEntry.EntryFullName;

        // ページ名：システムパス (リンクは実体に変換済)
        public string SystemPath => ArchiveEntry.SystemPath;

        // ページ名：ブックプレフィックス
        public string BookPrefix { get; private set; }

        // ページ名：スマート名用プレフィックス
        public string? Prefix { get; set; }

        // ブックのパス
        public string BookAddress => LoosePath.TrimEnd(BookPrefix);

        // ファイル情報：ファイル作成日
        public DateTime CreationTime => ArchiveEntry != null ? ArchiveEntry.CreationTime : default;

        // ファイル情報：最終更新日
        public DateTime LastWriteTime => ArchiveEntry != null ? ArchiveEntry.LastWriteTime : default;

        // ファイル情報：ファイルサイズ
        public long Length => ArchiveEntry.Length;

        // コンテンツ幅
        public double Width => Size.Width;

        // コンテンツ高
        public double Height => Size.Height;

        /// <summary>
        /// コンテンツサイズ
        /// </summary>
        public Size Size => _content.Size;

        /// <summary>
        /// コンテンツカラー
        /// </summary>
        public Color Color => _content.Color;

        /// <summary>
        /// ページの種類
        /// </summary>
        public PageType PageType => _content.PageType;

        // 表示中?
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public bool IsMarked
        {
            get { return _isMarked; }
            set { SetProperty(ref _isMarked, value); }
        }


        /// <summary>
        /// 要求状態
        /// </summary>
        public PageContentState State
        {
            get { return _content.State; }
            set { _content.State = value; }
        }

        /// <summary>
        /// 削除済フラグ
        /// </summary>
        public bool IsDeleted => ArchiveEntry.IsDeleted;


        #region Thumbnail

        private readonly PageThumbnail _thumbnailSource;


        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail => _thumbnailSource.Thumbnail;

        public bool IsThumbnailValid => _thumbnailSource.Thumbnail.IsValid;

        /// <summary>
        /// サムネイル読み込み
        /// </summary>
        public async Task<ImageSource?> LoadThumbnailAsync(CancellationToken token)
        {
            if (_disposedValue) return null;

            try
            {
                token.ThrowIfCancellationRequested();
                await _thumbnailSource.LoadAsync(token);
                return this.Thumbnail?.CreateImageSource();
            }
            catch
            {
                // nop.
                return null;
            }
        }

        #endregion


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _content.ContentChanged -= Content_ContentChanged;
                    _content.SizeChanged -= Content_SizeChanged;
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

        private void Content_ContentChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Content));
            RaisePropertyChanged(nameof(Color));
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Content_SizeChanged(object? sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(Size));
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }

        // TODO: これをPageのメソッドとして公開するのは？
        public async Task LoadContentAsync(CancellationToken token)
        {
            await _content.LoadAsync(token);
        }

        // TODO: PageLoader管理と競合している問題
        public void Unload()
        {
            _content.Unload();
            ContentChanged?.Invoke(this, EventArgs.Empty);
        }


        public override string ToString()
        {
            return $"Page:{Index}, {EntryFullName}";
        }

        public Page? GetPage()
        {
            return this;
        }

        public SearchValue GetValue(SearchPropertyProfile profile, string? parameter, CancellationToken token)
        {
            switch (profile.Name)
            {
                case "text":
                    return new StringSearchValue(EntryLastName);
                case "date":
                    return new DateTimeSearchValue(LastWriteTime);
                case "meta":
                    return new StringSearchValue(GetMetadata(parameter, token));
                default:
                    throw new NotSupportedException($"Not supported SearchProperty: {profile.Name}");
            }
        }

        public string GetMetadata(string? key, CancellationToken token)
        {
            // PictureInfo 取得
            var pictureInfo = _content.PictureInfo;
            if (pictureInfo is null)
            {
                 pictureInfo = _content.LoadPictureInfoAsync(token).Result;  // ## よろしくない？
            }
            if (pictureInfo is null) return "";

            var meta = pictureInfo.Metadata;
            if (meta is null) return "";

            // NOTE: ひとまず "tags" のみ対応
            // TODO: すべてのパラメータに対応
            if (key == "tags")
            {
                if (meta.TryGetValue(Media.Imaging.Metadata.BitmapMetadataKey.Tags, out var value))
                {
                    return MetadataValueTools.ToDispString(value) ?? "";
                }
            }

            return "";
        }


        #region Page functions


        // ページ名：ソート用分割
        public string[] GetEntryFullNameTokens()
        {
            return LoosePath.Split(EntryName);
        }

        // ページ名：プレフィックスを除いたフルパス
        public string GetSmartFullName()
        {
            return EntrySmartName.Replace("\\", " > ");
        }

        public string GetSmartDirectoryName()
        {
            return LoosePath.GetDirectoryName(EntrySmartName).Replace("\\", " > ");
        }

        // ファイルの場所を取得
        public string GetFilePlace()
        {
            return ArchiveEntry.GetFileSystemPath() ?? ArchiveEntry.Archiver.GetPlace();
        }

        // フォルダーを開く、で取得するパス
        public string GetFolderOpenPlace()
        {
            if (ArchiveEntry.Archiver is FolderArchive)
            {
                return GetFilePlace();
            }
            else
            {
                return GetFolderPlace();
            }
        }

        // フォルダーの場所を取得
        public string GetFolderPlace()
        {
            return ArchiveEntry.Archiver.GetSourceFileSystemPath();
        }

        /// <summary>
        /// can delete?
        /// </summary>
        public bool CanDelete()
        {
            return ArchiveEntry.CanDelete();
        }

        /// <summary>
        /// delete
        /// </summary>
        public async Task<bool> DeleteAsync()
        {
            return await ArchiveEntry.DeleteAsync();
        }

        public string GetRenameText()
        {
            return ArchiveEntry.GetRenameText();
        }

        public bool CanRename()
        {
            return ArchiveEntry.CanRename();
        }

        public async Task<bool> RenameAsync(string name)
        {
            var isSuccess = await ArchiveEntry.RenameAsync(name);
            RaiseNamePropertyChanged();
            FileInformation.Current.Update(); // TODO: 伝達方法がよろしくない
            return isSuccess;
        }

        private void RaiseNamePropertyChanged()
        {
            RaisePropertyChanged(nameof(EntryName));
            RaisePropertyChanged(nameof(EntryLastName));
            RaisePropertyChanged(nameof(EntrySmartName));
            RaisePropertyChanged(nameof(EntryFullName));
            RaisePropertyChanged(nameof(SystemPath));
        }

        #endregion


    }
}
