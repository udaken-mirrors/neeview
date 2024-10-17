﻿using NeeLaboratory.ComponentModel;
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
            
            ActivatePreExtractor();
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
        public bool IsDirectory => this.ArchiveEntryCollection.Archive is FolderArchive;

        // メディアアーカイバ？
        public bool IsMedia => ArchiveEntryCollection?.Archive is MediaArchive;


        // プレイリスト？
        public bool IsPlaylist => ArchiveEntryCollection?.Archive is PlaylistArchive;

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
                    DeactivatePreExtractor();
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

        public string GetArchiveDetail()
        {
            var archiver = ArchiveEntryCollection?.Archive;
            if (archiver == null)
            {
                return "";
            }

            var inner = archiver.Parent != null ? Properties.TextResources.GetString("Word.Inner") + " " : "";

            var extension = LoosePath.GetExtension(archiver.EntryName);

            var archiverType = ArchiveManager.GetArchiveType(archiver);
            return archiverType switch
            {
                ArchiveType.FolderArchive
                    => Properties.TextResources.GetString("ArchiveFormat.Folder"),
                ArchiveType.ZipArchive or ArchiveType.SevenZipArchive or ArchiveType.SusieArchive
                    => inner + Properties.TextResources.GetString("ArchiveFormat.CompressedFile") + $"({extension})",
                ArchiveType.PdfArchive
                    => inner + Properties.TextResources.GetString("ArchiveFormat.Pdf") + $"({extension})",
                ArchiveType.MediaArchive
                    => inner + Properties.TextResources.GetString("ArchiveFormat.Media") + $"({extension})",
                ArchiveType.PlaylistArchive
                    => Properties.TextResources.GetString("ArchiveFormat.Playlist"),
                _
                    => Properties.TextResources.GetString("ArchiveFormat.Unknown"),
            };
        }

        public string GetDetail()
        {
            string text = "";
            text += GetArchiveDetail() + "\n";
            text += Properties.TextResources.GetFormatString("BookAddressInfo.Page", Pages.Count);
            return text;
        }

        public string? GetFolderPlace()
        {
            return ArchiveEntryCollection.GetFolderPlace();
        }

        /// <summary>
        /// アーカイバ事前展開を許可
        /// </summary>
        private void ActivatePreExtractor()
        {
            foreach (var archiver in Pages.CollectArchive())
            {
                archiver.ActivatePreExtractor();
            }
        }

        /// <summary>
        /// アーカイバ事前展開を停止
        /// </summary>
        private void DeactivatePreExtractor()
        {
            foreach (var archiver in Pages.CollectArchive())
            {
                archiver.DeactivatePreExtractor();
                archiver.ClearRawData();
            }
        }

    }

}
