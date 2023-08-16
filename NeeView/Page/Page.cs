using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;

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



    [NotifyPropertyChanged]
    public partial class Page : IDisposable, INotifyPropertyChanged, IPageContentLoader, IPageThumbnailLoader, IHasPage, IRenameable
    {
        //private static PageContentFactory _pageContentFactory = new PageContentFactory();

        private int _index;
        private IPageContent _content;
        private bool _isVisibled;
        private bool _isMarked;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();


#if false
        public Page(string bookPrefix, ArchiveEntry archiveEntry, int index, BookMemoryService bookMemoryService)
            : this(bookPrefix, _pageContentFactory.Create(archiveEntry, bookMemoryService) )
        {
            Index = index;
        }
#endif


        public Page(string bookPrefix, IPageContent content)
        {
            BookPrefix = bookPrefix;

            _content = content;
            _content.ContentChanged += Content_ContentChanged;
            _content.SizeChanged += Content_SizeChanged;

            _thumbnailSource = PageThumbnailFactory.Create(_content);

#if false
            _disposables.Add(_content.SubscribePropertyChanged(nameof(PageContent.Entry),
                (s, e) => RaisePropertyChanged(nameof(Entry))));

            _contentLoader = _content.CreateContentLoader();
            _disposables.Add(_contentLoader.SubscribeLoaded(
                (s, e) => Loaded?.Invoke(this, EventArgs.Empty)));
#endif
        }





        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;

        [Subscribable]
        public event EventHandler? ContentChanged;

        [Subscribable]
        public event EventHandler? SizeChanged;



        public bool IsLoaded => _content.IsLoaded;

        [Obsolete("use Entry")]
        public ArchiveEntry ArchiveEntry => _content.ArchiveEntry;

        public ArchiveEntry Entry => _content.ArchiveEntry;

        /// <summary>
        /// コンテンツアクセサ。コンテンツを編集する場合はこのアクセサを介して操作を行う。
        /// </summary>
        //public PageContent ContentAccessor => _content;
        public IPageContent Content => _content;


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
        //public PageType PageType => _content is ArchiveContent ? PageType.Folder : PageType.File;
        public PageType PageType => PageType.File;

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


        #region Thumbnail

        private PageThumbnail _thumbnailSource;


        /// <summary>
        /// サムネイル
        /// </summary>
        public Thumbnail Thumbnail => _thumbnailSource.Thumbnail;
        //public Thumbnail Thumbnail { get; } = new Thumbnail();

        public bool IsThumbnailValid => _thumbnailSource.Thumbnail.IsValid;
        //public bool IsThumbnailValid => false;

        /// <summary>
        /// サムネイル読み込み
        /// </summary>
        public async Task<ImageSource?> LoadThumbnailAsync(CancellationToken token)
        {
            if (_disposedValue) return null;

            try
            {
                token.ThrowIfCancellationRequested();
                await _thumbnailSource.LoadThumbnailAsync(token);
                return this.Thumbnail?.ImageSource;
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
            return $"Page:{Index}";
        }

        public Page? GetPage()
        {
            return this;
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


        //public PageContent GetContentClone()
        //{
        //    return _content.Clone();
        //}


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

        #endregion


    }
}
