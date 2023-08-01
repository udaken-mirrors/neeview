using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using NeeLaboratory.ComponentModel;
using System.Windows.Media.Effects;

namespace NeeView
{
    public enum PageType
    {
        Folder,
        File,
    }

    public interface IHasPage
    {
        Page? GetPage();
    }



    /// <summary>
    /// ページ
    /// </summary>
    public class Page : BindableBase, IHasPage, IHasPageContent, IDisposable, IRenameable, IPageContentLoader, IPageThumbnailLoader
    {
        #region 開発用

        [Conditional("DEBUG")]
        private void DebugRaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            RaisePropertyChanged(name);
        }

        [Conditional("DEBUG")]
        public void DebugRaiseContentPropertyChanged()
        {
            DebugRaisePropertyChanged(nameof(ContentAccessor));
        }

        #endregion

        private readonly PageContent _content;
        private readonly IContentLoader _contentLoader;
        private bool _isVisibled;
        private bool _isMarked;
        private readonly DisposableCollection _disposables = new();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Page(string bookPrefix, PageContent content)
        {
            BookPrefix = bookPrefix;

            _content = content;
            _disposables.Add(_content.SubscribePropertyChanged(nameof(PageContent.Entry),
                (s, e) => RaisePropertyChanged(nameof(Entry))));

            _contentLoader = _content.CreateContentLoader();
            _disposables.Add(_contentLoader.SubscribeLoaded(
                (s, e) => Loaded?.Invoke(this, EventArgs.Empty)));
        }


        // コンテンツ更新イベント
        public event EventHandler? Loaded;

        public IDisposable SubscribeLoaded(EventHandler handler)
        {
            Loaded += handler;
            return new AnonymousDisposable(() => Loaded -= handler);
        }


        public bool IsLoaded => _content.IsLoaded;

        // アーカイブエントリ
        public ArchiveEntry Entry => _content.Entry;

        /// <summary>
        /// コンテンツアクセサ。コンテンツを編集する場合はこのアクセサを介して操作を行う。
        /// </summary>
        public PageContent ContentAccessor => _content;

        // 登録番号
        public int EntryIndex { get; set; }

        // ページ番号
        public int Index { get; set; }

        // TODO: 表示番号と内部番号のずれ
        public int IndexPlusOne => Index + 1;

        // 場所
        public string? Place { get; protected set; }

        // ページ名 : エントリ名
        public string EntryName => Entry.EntryFullName[BookPrefix.Length..];

        // ページ名：ファイル名のみ
        public string EntryLastName => Entry.EntryLastName;

        // ページ名：スマートパス
        public string EntrySmartName => Prefix == null ? EntryName : EntryName[Prefix.Length..];

        // ページ名：フルパス名 (リンクはそのまま)
        public string EntryFullName => Entry.EntryFullName;

        // ページ名：システムパス (リンクは実体に変換済)
        public string SystemPath => Entry.SystemPath;

        // ページ名：ブックプレフィックス
        public string BookPrefix { get; private set; }

        // ページ名：スマート名用プレフィックス
        public string? Prefix { get; set; }

        // ブックのパス
        public string BookAddress => LoosePath.TrimEnd(BookPrefix);

        // ファイル情報：ファイル作成日
        public DateTime CreationTime => Entry != null ? Entry.CreationTime : default;

        // ファイル情報：最終更新日
        public DateTime LastWriteTime => Entry != null ? Entry.LastWriteTime : default;

        // ファイル情報：ファイルサイズ
        public long Length => Entry.Length;

        // コンテンツ幅
        public double Width => Size.Width;

        // コンテンツ高
        public double Height => Size.Height;

        /// <summary>
        /// コンテンツサイズ。カスタムサイズが指定されているときはここで反映される
        /// </summary>
        public Size Size
        {
            get { return CustomSize.Current.TransformToCustomSize(_content.Size); }
        }

        /// <summary>
        /// ページの種類
        /// </summary>
        public PageType PageType => _content is ArchiveContent ? PageType.Folder : PageType.File;

        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail => _content.Thumbnail;

        public bool IsThumbnailValid => _content.Thumbnail.IsValid;

        // 表示中?
        public bool IsVisibled
        {
            get { return _isVisibled; }
            set { SetProperty(ref _isVisibled, value); }
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
        public bool IsDeleted => Entry.IsDeleted;


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                    Loaded = null;
                    ResetPropertyChanged();
                    _contentLoader.Dispose();
                    _content.Dispose();
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

        // ToString
        public override string? ToString()
        {
            var name = _content.ToString();
            if (name == null) return base.ToString();
            return $"{name}: State={State}";
        }


        /// <summary>
        /// コンテンツ読み込み
        /// </summary>
        public async Task LoadContentAsync(CancellationToken token)
        {
            if (_disposedValue) return;

            try
            {
                token.ThrowIfCancellationRequested();
                await _contentLoader.LoadContentAsync(token);
                RaisePropertyChanged(nameof(ContentAccessor));
            }
            catch
            {
                // nop.
            }
        }

        /// <summary>
        /// コンテンツを閉じる
        /// </summary>
        public void UnloadContent()
        {
            Debug.Assert(State == PageContentState.None);

            _contentLoader.UnloadContent();
            RaisePropertyChanged(nameof(ContentAccessor));
        }


        /// <summary>
        /// サムネイル読み込み
        /// </summary>
        public async Task<ImageSource?> LoadThumbnailAsync(CancellationToken token)
        {
            if (_disposedValue) return null;

            try
            {
                token.ThrowIfCancellationRequested();
                await _contentLoader.LoadThumbnailAsync(token);
                return this.Thumbnail?.ImageSource;
            }
            catch
            {
                // nop.
                return null;
            }
        }


        // ImageSource取得
        public ImageSource? GetContentImageSource()
        {
            return (_content as BitmapContent)?.ImageSource;
        }

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
            return Entry.GetFileSystemPath() ?? Entry.Archiver.GetPlace();
        }

        // フォルダーを開く、で取得するパス
        public string GetFolderOpenPlace()
        {
            if (Entry.Archiver is FolderArchive)
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
            return Entry.Archiver.GetSourceFileSystemPath();
        }

        public Page GetPage()
        {
            return this;
        }

        public PageContent GetContentClone()
        {
            return _content.Clone();
        }

        public async Task InitializeEntryAsync(CancellationToken token)
        {
            if (_contentLoader is IHasInitializeEntry initializer)
            {
                await initializer.InitializeEntryAsync(token);
            }
        }

        /// <summary>
        /// can delete?
        /// </summary>
        public bool CanDelete()
        {
            return Entry.CanDelete();
        }

        /// <summary>
        /// delete
        /// </summary>
        public async Task<bool> DeleteAsync()
        {
            return await Entry.DeleteAsync();
        }

        /// <summary>
        /// Create Page visual for Dialog thumbnail.
        /// </summary>
        /// <returns>Image</returns>
        public async Task<Image> CreatePageVisualAsync()
        {
            var imageSource = await LoadThumbnailAsync(CancellationToken.None);

            var image = new Image();
            image.Source = imageSource;
            image.Effect = new DropShadowEffect()
            {
                Opacity = 0.5,
                ShadowDepth = 2,
                RenderingBias = RenderingBias.Quality
            };
            image.MaxWidth = 96;
            image.MaxHeight = 96;

            return image;
        }


        public string GetRenameText()
        {
            return Entry.GetRenameText();
        }

        public bool CanRename()
        {
            return Entry.CanRename();
        }

        public async Task<bool> RenameAsync(string name)
        {
            var isSuccess = await Entry.RenameAsync(name);
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
    }

}
