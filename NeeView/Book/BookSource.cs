using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;

namespace NeeView
{
    public partial class BookSource : IDisposable
    {
        public BookSource(ArchiveEntryCollection archiveEntryCollection, BookPageCollection pages, BookMemoryService bookMemoryService)
        {
            ArchiveEntryCollection = archiveEntryCollection;
            Pages = pages;
            BookMemoryService = bookMemoryService;

            _isRecursiveFolder = ArchiveEntryCollection.Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
        }


        // 再読み込みを要求
        [Subscribable]
        public event EventHandler? DirtyBook;


        public BookMemoryService BookMemoryService { get; private set; }    

        // この本のアーカイバ
        public ArchiveEntryCollection ArchiveEntryCollection { get; private set; }

        // この本の場所
        public string Path => this.ArchiveEntryCollection.Path;

        // この本はディレクトリ？
        public bool IsDirectory => this.ArchiveEntryCollection.Archiver is FolderArchive;

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archiver is MediaArchiver;


        // プレイリスト？
        public bool IsPlaylist => ArchiveEntryCollection?.Archiver is PlaylistArchive;

        /// <summary>
        /// 読み込まれなかったサブフォルダ数。再帰判定用
        /// </summary>
        public int SubFolderCount { get; set; }

        // この本を構成するページ
        public BookPageCollection Pages { get; private set; }


        // サブフォルダー読み込み
        private bool _isRecursiveFolder;
        public bool IsRecursiveFolder
        {
            get { return _isRecursiveFolder; }
            set
            {
                if (_isRecursiveFolder != value)
                {
                    _isRecursiveFolder = value;
                    DirtyBook?.Invoke(this, EventArgs.Empty);
                }
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this.DirtyBook = null;
                    BookMemoryService.Dispose();
                    Pages.Dispose();
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

        public string GetArchiverDetail()
        {
            var archiver = ArchiveEntryCollection?.Archiver;
            if (archiver == null)
            {
                return "";
            }

            var inner = archiver.Parent != null ? Properties.Resources.Word_Inner + " " : "";

            var extension = LoosePath.GetExtension(archiver.EntryName);

            var archiverType = ArchiverManager.GetArchiverType(archiver);
            return archiverType switch
            {
                ArchiverType.FolderArchive
                    => Properties.Resources.ArchiveFormat_Folder,
                ArchiverType.ZipArchiver or ArchiverType.SevenZipArchiver or ArchiverType.SusieArchiver
                    => inner + Properties.Resources.ArchiveFormat_CompressedFile + $"({extension})",
                ArchiverType.PdfArchiver
                    => inner + Properties.Resources.ArchiveFormat_Pdf + $"({extension})",
                ArchiverType.MediaArchiver
                    => inner + Properties.Resources.ArchiveFormat_Media + $"({extension})",
                ArchiverType.PlaylistArchiver
                    => Properties.Resources.ArchiveFormat_Playlist,
                _
                    => Properties.Resources.ArchiveFormat_Unknown,
            };
        }

        public string GetDetail()
        {
            string text = "";
            text += GetArchiverDetail() + "\n";
            text += string.Format(Properties.Resources.BookAddressInfo_Page, Pages.Count);
            return text;
        }

        public string? GetFolderPlace()
        {
            return ArchiveEntryCollection.GetFolderPlace();
        }
    }

}
