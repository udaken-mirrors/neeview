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

            ArchiveEntry entry = new ArchiveEntry()
            {
                IsValid = true,
                Archiver = this,
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

        private ArchiveEntry GetTargetEntry(ArchiveEntry entry)
        {
            var target = entry.Instance as ArchiveEntry;
            if (target is null) throw new InvalidCastException();

            return target;
        }

        // ストリームを開く
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            return GetTargetEntry(entry).OpenEntry();
        }

        // ファイルパス取得
        public override string? GetFileSystemPath(ArchiveEntry entry)
        {
            return GetTargetEntry(entry).GetFileSystemPath();
        }

        // ファイル出力
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            GetTargetEntry(entry).ExtractToFile(exportFileName, isOverwrite);
        }
    }
}

