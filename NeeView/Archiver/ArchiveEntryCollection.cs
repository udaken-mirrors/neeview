using System;
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
    /// 指定パス以下のArchiveEntryの収集
    /// </summary>
    public class ArchiveEntryCollection
    {
        private readonly ArchiveEntryCollectionMode _mode;
        private readonly ArchiveEntryCollectionMode _modeIfArchive;
        private readonly bool _ignoreCache;
        private List<ArchiveEntryNode>? _entries;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">対象のパス</param>
        /// <param name="mode">標準再帰モード</param>
        /// <param name="modeIfArchive">圧縮ファイルの再帰モード</param>
        /// <param name="option"></param>
        public ArchiveEntryCollection(string path, ArchiveEntryCollectionMode mode, ArchiveEntryCollectionMode modeIfArchive, ArchiveEntryCollectionOption option)
        {
            Path = LoosePath.TrimEnd(path);
            Mode = mode;
            _mode = mode;
            _modeIfArchive = modeIfArchive;
            _ignoreCache = option.HasFlag(ArchiveEntryCollectionOption.IgnoreCache);
        }

        public string Path { get; }
        public Archiver? Archiver { get; private set; }

        public ArchiveEntryCollectionMode Mode { get; private set; }

        /// <summary>
        /// ArchiveEntry収集
        /// </summary>
        public async Task<List<ArchiveEntryNode>> GetEntriesAsync(CancellationToken token)
        {
            if (_entries != null) return _entries;

            var rootEntry = await ArchiveEntryUtility.CreateAsync(Path, token);

            Archiver? rootArchiver;
            string rootArchiverPath;

            if (rootEntry.IsFileSystem)
            {
                if (rootEntry.IsDirectory)
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(StaticFolderArchive.Default.CreateArchiveEntry(Path), _ignoreCache, token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, _ignoreCache, token);
                    rootArchiverPath = "";
                }
            }
            else
            {
                if (rootEntry.IsArchive() || rootEntry.IsMedia())
                {
                    rootArchiver = await ArchiverManager.Current.CreateArchiverAsync(rootEntry, _ignoreCache, token);
                    rootArchiverPath = "";
                }
                else
                {
                    rootArchiver = rootEntry.Archiver;
                    rootArchiverPath = rootEntry.EntryName;
                }
            }

            if (rootArchiver is null)
            {
                return new List<ArchiveEntryNode>() { new ArchiveEntryNode(null, rootEntry) };
            }

            Archiver = rootArchiver;

            Mode = Archiver.IsFileSystem ? _mode : _modeIfArchive;

            var includeSubDirectories = Mode == ArchiveEntryCollectionMode.IncludeSubDirectories || Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
            var entries = (await rootArchiver.GetEntriesAsync(rootArchiverPath, includeSubDirectories, token)).Select(e => new ArchiveEntryNode(null, e)).ToList();

            var includeAllSubDirectories = Mode == ArchiveEntryCollectionMode.IncludeSubArchives;
            if (includeAllSubDirectories)
            {
                entries = await GetSubArchivesEntriesAsync(entries, token);
            }

            _entries = entries;
            return _entries;
        }


        private async Task<List<ArchiveEntryNode>> GetSubArchivesEntriesAsync(List<ArchiveEntryNode> entries, CancellationToken token)
        {
            var result = new List<ArchiveEntryNode>();

            foreach (var entry in entries)
            {
                result.Add(entry);

                if (entry.ArchiveEntry.IsArchive())
                {
                    // 無限ループを避けるためショートカットは除外する
                    if (entry.ArchiveEntry.IsShortcut)
                    {
                        continue;
                    }

                    try
                    {
                        var subArchive = await ArchiverManager.Current.CreateArchiverAsync(entry.ArchiveEntry, _ignoreCache, token);
                        var subEntries = (await subArchive.GetEntriesAsync(token)).Select(e => new ArchiveEntryNode(entry, e)).ToList();
                        result.AddRange(await GetSubArchivesEntriesAsync(subEntries, token));
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine($"ArchiveEntryCollection.Skip: {entry.ArchiveEntry.EntryName}");
                    }
                }
            }

            return result;
        }


        // filter: ページとして画像ファイルのみリストアップ
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereImageAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.Where(e => e.ArchiveEntry.IsImage()).ToList();
        }

        // filter: ページとして画像ファイルとアーカイブをリストアップ
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereImageAndArchiveAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            if (Mode == ArchiveEntryCollectionMode.CurrentDirectory)
            {
                return entries.Where(e => e.ArchiveEntry.IsImage() || e.ArchiveEntry.IsBook()).ToList();
            }
            else
            {
                return entries.WherePageAll().Where(e => e.ArchiveEntry.IsImage() || e.ArchiveEntry.IsBook()).ToList();
            }
        }

        // filter: ページとしてすべてのファイルをリストアップ。フォルダーは空きフォルダーのみリストアップ
        public async Task<List<ArchiveEntryNode>> GetEntriesWherePageAllAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.WherePageAll().ToList();
        }

        // filter: 含まれるサブアーカイブのみ抽出
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereSubArchivesAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            return entries.Where(e => e.ArchiveEntry.IsArchive() || e.ArchiveEntry.IsMedia()).ToList();
        }

        // filter: 含まれるブックを抽出
        public async Task<List<ArchiveEntryNode>> GetEntriesWhereBookAsync(CancellationToken token)
        {
            var entries = await GetEntriesAsync(token);
            if (Mode == ArchiveEntryCollectionMode.CurrentDirectory)
            {
                return entries.Where(e => e.ArchiveEntry.IsBook()).ToList();
            }
            else
            {
                return entries.Where(e => e.ArchiveEntry.IsBook() && !e.ArchiveEntry.IsArchiveDirectory()).ToList();
            }
        }

        /// <summary>
        /// フォルダーリスト上での親フォルダーを取得
        /// </summary>
        public string? GetFolderPlace()
        {
            if (Path == null || Archiver == null)
            {
                return null;
            }

            if (Archiver == null)
            {
                Debug.Assert(false, "Invalid operation");
                return null;
            }

            if (Mode == ArchiveEntryCollectionMode.IncludeSubArchives)
            {
                return LoosePath.GetDirectoryName(Archiver.RootArchiver?.SystemPath);
            }
            else if (Mode == ArchiveEntryCollectionMode.IncludeSubDirectories)
            {
                if (Archiver.Parent != null)
                {
                    return Archiver.Parent.SystemPath;
                }
                else
                {
                    return LoosePath.GetDirectoryName(Archiver.SystemPath);
                }
            }
            else
            {
                return LoosePath.GetDirectoryName(Path);
            }
        }
    }

    public static class ArchiveEntryCollectionExtensions
    {
        /// <summary>
        /// filter: ディレクトリとなるエントリをすべて除外
        /// </summary>
        public static IEnumerable<ArchiveEntryNode> WherePageAll(this IEnumerable<ArchiveEntryNode> source)
        {
            var directories = source.Select(e => LoosePath.GetDirectoryName(e.ArchiveEntry.SystemPath)).Distinct().ToList();
            return source.Where(e => e.ArchiveEntry.IsShortcut || !directories.Contains(e.ArchiveEntry.SystemPath));
        }

        /// <summary>
        /// ArchiveEntryNode リストから ArchiveEntry リストを取得する
        /// </summary>
        public static List<ArchiveEntry> ToArchiveEntryCollection(this IEnumerable<ArchiveEntryNode> source)
        {
            return source.Select(e => e.ArchiveEntry).ToList();
        }
    }
}
