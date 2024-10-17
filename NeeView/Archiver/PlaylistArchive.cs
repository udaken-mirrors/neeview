﻿using NeeView;
using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：プレイリスト方式
    /// </summary>
    public class PlaylistArchive : Archiver
    {
        public const string Extension = ".nvpls";


        public PlaylistArchive(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public static bool IsSupportExtension(string path)
        {
            return LoosePath.GetExtension(path) == Extension;
        }

        public override string ToString()
        {
            return Properties.TextResources.GetString("Archiver.Playlist");
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // リスト取得
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var playlist = PlaylistSourceTools.Load(Path);
            var list = new List<ArchiveEntry>();

            foreach (var item in playlist.Items)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    var entry = await CreateEntryAsync(item, list.Count, token);
                    list.Add(entry);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            return list;
        }

        private async Task<ArchiveEntry> CreateEntryAsync(PlaylistSourceItem item, int id, CancellationToken token)
        {
            var targetPath = item.Path;

            // プレイリストに動画ブックの特殊形式 (/path/to/movie.mp4/movie.mp4) があるときの補正
            if (ArchiverManager.Current.GetSupportedType(targetPath) == ArchiverType.MediaArchiver && !File.Exists(targetPath))
            {
                var targetDirectory = LoosePath.GetDirectoryName(targetPath);
                if (ArchiverManager.Current.GetSupportedType(targetDirectory) == ArchiverType.MediaArchiver)
                {
                    targetPath = targetDirectory;
                }
            }

            var innerEntry = await ArchiveEntryUtility.CreateAsync(targetPath, token);

            var entry = new ArchiveEntry(this)
            {
                IsValid = true,
                Id = id,
                RawEntryName = item.Name,
                Length = innerEntry.Length,
                CreationTime = innerEntry.CreationTime,
                LastWriteTime = innerEntry.LastWriteTime,
                InnerEntry = innerEntry,
            };

            return entry;
        }

        public override string GetPlacePath(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archiver == this);
            Debug.Assert(entry.InnerEntry is not null);
            return entry.InnerEntry.PlacePath;
        }

        public override string GetSystemPath(ArchiveEntry entry) 
        {
            Debug.Assert(entry.Archiver == this);
            Debug.Assert(entry.InnerEntry is not null);
            return entry.InnerEntry.SystemPath;
        }

        /// <summary>
        /// エントリの実体パスを取得
        /// </summary>
        /// <param name="entry">エントリ</param>
        /// <returns>実体パス。アーカイブパス等実在しない場合は null</returns>
        public override string? GetEntityPath(ArchiveEntry entry)
        {
            Debug.Assert(entry.Archiver == this);
            Debug.Assert(entry.InnerEntry is not null);
            return entry.InnerEntry.EntityPath;
        }

        // ストリームを開く
        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            Debug.Assert(entry.Archiver == this);
            Debug.Assert(entry.InnerEntry is not null);
            return await entry.InnerEntry.OpenEntryAsync(token);
        }

        // ファイル出力
        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            Debug.Assert(entry.Archiver == this);
            Debug.Assert(entry.InnerEntry is not null);
            await entry.InnerEntry.ExtractToFileAsync(exportFileName, isOverwrite, token);
        }
    }
}

