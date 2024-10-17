﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイブ内パスを含むパス記述に対応した処理
    /// </summary>
    public static class ArchiveEntryUtility
    {
        /// <summary>
        /// 仮エントリ―作成
        /// </summary>
        /// <remarks>
        /// 指定したパスがファイルシステム上に存在すると仮定して発行する。アーカイブ内等の実エントリーにするにはCreateAsync()を使用する。
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        public static ArchiveEntry CreateTemporaryEntry(string path)
        {
            return StaticFolderArchive.Default.CreateArchiveEntry(path, true);
        }

        /// <summary>
        /// パスから完全なArchiveEntryを作成
        /// </summary>
        public static async Task<ArchiveEntry> CreateAsync(string path, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var query = new QueryPath(path);

            if (File.Exists(path) || Directory.Exists(path))
            {
                return StaticFolderArchive.Default.CreateArchiveEntry(path);
            }
            else
            {
                try
                {
                    var parts = LoosePath.Split(path);
                    string? archivePath = null;

                    foreach (var part in parts)
                    {
                        archivePath = LoosePath.Combine(archivePath, part);

                        if (File.Exists(archivePath))
                        {
                            var archiver = await ArchiveManager.Current.CreateArchiveAsync(StaticFolderArchive.Default.CreateArchiveEntry(archivePath), false, token);
                            var entries = await archiver.GetEntriesAsync(token);

                            // メディア アーカイブの場合、ページ指定は無効
                            if (archiver is MediaArchive)
                            {
                                return entries.First();
                            }

                            var entryName = path[archivePath.Length..].TrimStart(LoosePath.Separators);
                            var entry = entries.FirstOrDefault(e => e.EntryName == entryName);
                            if (entry != null)
                            {
                                return entry;
                            }
                            else
                            {
                                return await CreateInnerAsync(archiver, entryName, token);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new FileNotFoundException(string.Format(Properties.TextResources.GetString("FileNotFoundException.Message"), path), ex);
                }
            }

            throw new FileNotFoundException(string.Format(Properties.TextResources.GetString("FileNotFoundException.Message"), path));
        }

        /// <summary>
        /// アーカイブ内のエントリーを返す。
        /// 入れ子になったアーカイブの場合、再帰処理する。
        /// </summary>
        private static async Task<ArchiveEntry> CreateInnerAsync(Archive archiver, string entryName, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var entries = await archiver.GetEntriesAsync(token);

            var entry = entries.FirstOrDefault(e => e.EntryName == entryName);
            if (entry != null) return entry;

            var parts = LoosePath.Split(entryName);
            string? archivePath = null;

            foreach (var part in parts)
            {
                archivePath = LoosePath.Combine(archivePath, part);

                entry = entries.FirstOrDefault(e => e.EntryName == archivePath && e.IsArchive());
                if (entry != null)
                {
                    var subArchive = await ArchiveManager.Current.CreateArchiveAsync(entry, false, token);
                    var subEntryName = entryName[archivePath.Length..].TrimStart(LoosePath.Separators);
                    return await CreateInnerAsync(subArchive, subEntryName, token);
                }
            }

            throw new FileNotFoundException();
        }

        /// <summary>
        /// ブックサムネイルに使用するエントリを取得
        /// </summary>
        /// <param name="source">対象のエントリ</param>
        /// <param name="depth">検索範囲</param>
        public static async Task<ArchiveEntry?> CreateFirstImageArchiveEntryAsync(ArchiveEntry source, int depth, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                List<ArchiveEntry> entries;
                if (!source.IsFileSystem && source.IsDirectory)
                {
                    entries = (await source.Archive.GetEntriesAsync(token))
                        .Where(e => e.EntryName.StartsWith(LoosePath.TrimDirectoryEnd(source.EntryName)))
                        .ToList();
                }
                else
                {
                    var archiver = await ArchiveManager.Current.CreateArchiveAsync(source, false, token);
                    await archiver.WaitFileReadableAsync(TimeSpan.FromMilliseconds(1000), token);
                    entries = await archiver.GetEntriesAsync(token);
                }

                // NOTE: ToList()で確定させる。もともとList<>であるが、これがないと次の行以降で Collection was modified; enumeration operation may not execute が発生することがある
                // TODO: 原因の調査
                entries = EntrySort.SortEntries(entries, PageSortMode.FileName).ToList();

                // メディアを除外した最初の画像ページを取得
                var select = entries.FirstOrDefault(e => e.IsImage(false));
                if (select != null)
                {
                    return select;
                }

                if (depth > 1)
                {
                    // NOTE: 検索サブディレクトリ数も depth で制限
                    foreach (var entry in entries.Where(e => e.IsArchive()).Take(depth))
                    {
                        select = await CreateFirstImageArchiveEntryAsync(entry, depth - 1, token);
                        if (select != null)
                        {
                            return select;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        /// <summary>
        /// アーカイブパスの存在チェック
        /// </summary>
        /// <param name="path"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<bool> ExistsAsync(string path, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            try
            {
                var entry = await CreateAsync(path, token);
                return entry != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }


        /// <summary>
        /// 実在するディレクトリまで遡る
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? GetExistDirectoryName(string path)
        {
            if (Directory.Exists(path))
            {
                return path;
            }

            while (!string.IsNullOrEmpty(path))
            {
                path = LoosePath.GetDirectoryName(path);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// 実在するエントリーまで遡る
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string? GetExistEntryName(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                return path;
            }

            while (!string.IsNullOrEmpty(path))
            {
                path = LoosePath.GetDirectoryName(path);
                if (Directory.Exists(path) || File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

    }

}
