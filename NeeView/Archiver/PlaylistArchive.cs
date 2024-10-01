using NeeView;
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


        public override bool IsFileSystem { get; } = false;


        public override bool IsFileSystemEntry(ArchiveEntry entry)
        {
            return entry.Instance is ArchiveEntry innerEntry && innerEntry.IsFileSystem;
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

            if (FileShortcut.IsShortcut(item.Path))
            {
                var shortcut = new FileShortcut(item.Path);
                if (shortcut.TryGetTargetPath(out var target))
                {
                    targetPath = target;
                }
            }

            var innerEntry = await ArchiveEntryUtility.CreateAsync(targetPath, token);

            var entry = new ArchiveEntry(this)
            {
                IsValid = true,
                Id = id,
                RawEntryName = item.Name,
                Link = targetPath,
                Instance = innerEntry,
                Length = innerEntry.Length,
                CreationTime = innerEntry.CreationTime,
                LastWriteTime = innerEntry.LastWriteTime,
            };

            return entry;
        }

        private static ArchiveEntry GetTargetEntry(ArchiveEntry entry)
        {
            var target = entry.Instance as ArchiveEntry ?? throw new InvalidCastException();
            return target;
        }

        // ストリームを開く
        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            return await GetTargetEntry(entry).OpenEntryAsync(token);
        }

        // ファイルパス取得
        public override string? GetFileSystemPath(ArchiveEntry entry)
        {
            return GetTargetEntry(entry).GetFileSystemPath();
        }

        // ファイル出力
        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            await GetTargetEntry(entry).ExtractToFileAsync(exportFileName, isOverwrite, token);
        }
    }
}

