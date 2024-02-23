﻿using NeeLaboratory.Linq;
using NeeView.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：通常ファイル
    /// ディレクトリをアーカイブとみなしてアクセスする
    /// </summary>
    public class FolderArchive : Archiver
    {
        public FolderArchive(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public override bool IsFileSystem { get; } = true;


        public override string ToString()
        {
            return "Folder";
        }

        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // リスト取得
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            // Pathがない場合は汎用アーカイブなのでリスト作成は行わない
            if (string.IsNullOrEmpty(Path))
            {
                Debug.Fail("If there is no Path, it is a general-purpose archive and does not create a list.");
                return new List<ArchiveEntry>();
            }

            token.ThrowIfCancellationRequested();

            var list = new List<ArchiveEntry>();

            var directory = new DirectoryInfo(Path);
            foreach (var info in directory.EnumerateFileSystemInfos())
            {
                token.ThrowIfCancellationRequested();

                if (!FileIOProfile.Current.IsFileValid(info.Attributes))
                {
                    continue;
                }

                var entry = CreateArchiveEntry(info, list.Count);
                list.Add(entry);
            }

            return await Task.FromResult(list);
        }

        protected ArchiveEntry CreateArchiveEntry(FileSystemInfo info, int id)
        {
            if (info is DirectoryInfo directoryInfo)
            {
                return CreateArchiveEntry(directoryInfo, id);
            }
            else if (info is FileInfo fileInfo)
            {
                return CreateArchiveEntry(fileInfo, id);
            }
            else
            {
                throw new ArgumentException("not exists entry.", nameof(info));
            }
        }

        protected ArchiveEntry CreateArchiveEntry(DirectoryInfo info, int id)
        {
            return CreateCommonArchiveEntry(info, id);
        }

        protected ArchiveEntry CreateArchiveEntry(FileInfo info, int id)
        {
            var entry = CreateCommonArchiveEntry(info, id);

            if (FileShortcut.IsShortcut(info.Name))
            {
                var shortcut = new FileShortcut(info);
                if (shortcut.TryGetTarget(out var target))
                {
                    if (target.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        entry.Link = target.FullName;
                        entry.Length = -1;
                        entry.CreationTime = target.CreationTime;
                        entry.LastWriteTime = target.LastWriteTime;
                    }
                    else
                    {
                        var fileInfo = (FileInfo)target;
                        entry.Link = target.FullName;
                        entry.Length = fileInfo.Length;
                        entry.CreationTime = target.CreationTime;
                        entry.LastWriteTime = target.LastWriteTime;
                    }
                }
            }

            return entry;
        }

        private ArchiveEntry CreateCommonArchiveEntry(FileSystemInfo info, int id)
        {
            var name = string.IsNullOrEmpty(Path) ? info.FullName : info.FullName[Path.Length..].TrimStart('\\', '/');

            var entry = new ArchiveEntry(this)
            {
                IsValid = true,
                Id = id,
                RawEntryName = name,
                Length = info is FileInfo fileInfo ? fileInfo.Length : -1,
                CreationTime = info.CreationTime,
                LastWriteTime = info.LastWriteTime,
            };

            return entry;
        }

        // ストリームを開く
        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            return await Task.FromResult(new FileStream(entry.Link ?? GetFileSystemPath(entry), FileMode.Open, FileAccess.Read));
        }

        // ファイルパス取得
        public override string GetFileSystemPath(ArchiveEntry entry)
        {
            return System.IO.Path.Combine(Path, entry.EntryName);
        }

        // ファイル出力
        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            await FileIO.CopyFileAsync(GetFileSystemPath(entry), exportFileName, isOverwrite, token);
        }

        /// <summary>
        /// exists?
        /// </summary>
        public override bool Exists(ArchiveEntry entry)
        {
            if (entry.Archiver != this) return false;
            if (entry.IsDeleted) return false;

            var path = entry.GetFileSystemPath();
            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// can delete
        /// </summary>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override bool CanDelete(List<ArchiveEntry> entries)
        {
            if (entries.Any(e => e.Archiver != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));

            // NOTE: 実際に削除可能かは調べない。削除で失敗させる。
            return entries.All(e => e.GetFileSystemPath() is not null);
        }

        /// <summary>
        /// delete entries
        /// </summary>
        /// <exception cref="ArgumentException">Not registered with this archiver.</exception>
        public override async Task<bool> DeleteAsync(List<ArchiveEntry> entries)
        {
            if (entries.Any(e => e.Archiver != this)) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entries));

            var paths = entries.Select(e => e.GetFileSystemPath()).WhereNotNull().ToList();
            if (!paths.Any()) return false;

            ClearEntryCache();
            try
            {
                return await FileIO.DeleteAsync(paths);
            }
            finally
            {
                foreach (var entry in entries)
                {
                    if (!entry.Exists())
                    {
                        entry.IsDeleted = true;
                    }
                }
            }
        }

        /// <summary>
        /// can rename?
        /// </summary>
        public override bool CanRename(ArchiveEntry entry)
        {
            if (entry.Archiver != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));

            return true;
        }

        /// <summary>
        /// rename
        /// </summary>
        public override async Task<bool> RenameAsync(ArchiveEntry entry, string name)
        {
            if (entry.Archiver != this) throw new ArgumentException("There are elements not registered with this archiver.", nameof(entry));

            var src = entry.GetFileSystemPath();
            if (src is null) return false;

            // TODO: 名前の補正処理をここで？UI呼ばせるのはよろしくないのでは？
            var dst = FileIO.CreateRenameDst(src, name, true);
            if (dst is null) return false;

            var isSuccess = await FileIO.RenameAsync(src, dst, true);
            if (isSuccess)
            {
                var rawEntryName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(entry.RawEntryName) ?? "", System.IO.Path.GetFileName(dst));
                entry.RawEntryName = rawEntryName;
            }

            return isSuccess;
        }
    }
}
