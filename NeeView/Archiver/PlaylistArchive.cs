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


        public static bool IsSupportExtension(string path)
        {
            return LoosePath.GetExtension(path) == Extension;
        }

        public override string ToString()
        {
            return "Playlist";
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

            if (FileShortcut.IsShortcut(item.Path))
            {
                var shortcut = new FileShortcut(item.Path);
                if (shortcut.TryGetTargetPath(out var target))
                {
                    targetPath = target;
                }
            }

            var innterEntry = await ArchiveEntryUtility.CreateAsync(targetPath, token);

            var entry = new ArchiveEntry(this)
            {
                IsValid = true,
                Id = id,
                RawEntryName = item.Name,
                Link = targetPath,
                Instance = innterEntry,
                Length = innterEntry.Length,
                CreationTime = innterEntry.CreationTime,
                LastWriteTime = innterEntry.LastWriteTime,
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

