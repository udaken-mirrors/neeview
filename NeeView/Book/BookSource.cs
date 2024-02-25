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
                    SleepArchivers();
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

            var inner = archiver.Parent != null ? Properties.TextResources.GetString("Word.Inner") + " " : "";

            var extension = LoosePath.GetExtension(archiver.EntryName);

            var archiverType = ArchiverManager.GetArchiverType(archiver);
            return archiverType switch
            {
                ArchiverType.FolderArchive
                    => Properties.TextResources.GetString("ArchiveFormat.Folder"),
                ArchiverType.ZipArchiver or ArchiverType.SevenZipArchiver or ArchiverType.SusieArchiver
                    => inner + Properties.TextResources.GetString("ArchiveFormat.CompressedFile") + $"({extension})",
                ArchiverType.PdfArchiver
                    => inner + Properties.TextResources.GetString("ArchiveFormat.Pdf") + $"({extension})",
                ArchiverType.MediaArchiver
                    => inner + Properties.TextResources.GetString("ArchiveFormat.Media") + $"({extension})",
                ArchiverType.PlaylistArchiver
                    => Properties.TextResources.GetString("ArchiveFormat.Playlist"),
                _
                    => Properties.TextResources.GetString("ArchiveFormat.Unknown"),
            };
        }

        public string GetDetail()
        {
            string text = "";
            text += GetArchiverDetail() + "\n";
            text += string.Format(Properties.TextResources.GetString("BookAddressInfo.Page"), Pages.Count);
            return text;
        }

        public string? GetFolderPlace()
        {
            return ArchiveEntryCollection.GetFolderPlace();
        }

        /// <summary>
        /// アーカイバー休眠
        /// </summary>
        private void SleepArchivers()
        {
            foreach (var archiver in Pages.CollectArchiver())
            {
                archiver.Sleep();
                archiver.ResetRawData();
            }
        }

    }

}
