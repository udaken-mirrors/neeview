using NeeView.Susie;
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
    /// アーカイバー：Susieアーカイバー
    /// </summary>
    public class SusieArchiver : Archiver
    {
        private SusieArchivePluginAccessor? _susiePlugin;
        private readonly object _lock = new();


        public SusieArchiver(string path, ArchiveEntry? source) : base(path, source)
        {
        }


        public override string? ToString()
        {
            return _susiePlugin?.Plugin.Name ?? "(none)";
        }

        // サポート判定
        public override bool IsSupported()
        {
            return GetPlugin() != null;
        }

        // 対応プラグイン取得
        public SusieArchivePluginAccessor? GetPlugin()
        {
            if (_susiePlugin == null)
            {
                _susiePlugin = SusiePluginManager.Current.GetArchivePluginAccessor(Path, null, true);
            }
            return _susiePlugin;
        }

        // エントリーリストを得る
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var plugin = GetPlugin() ?? throw new NotSupportedException($"not archive: {Path}");
            var entries = plugin.GetArchiveEntries(Path) ?? throw new NotSupportedException();
            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            for (int id = 0; id < entries.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = entries[id];

                var archiveEntry = new ArchiveEntry(this)
                {
                    IsValid = true,
                    Id = id,
                    RawEntryName = (entry.Path.TrimEnd('\\', '/') + "\\" + entry.FileName).TrimStart('\\', '/'),
                    Length = entry.FileSize,
                    LastWriteTime = entry.TimeStamp,
                    Instance = entry,
                };

                if (!entry.IsDirectory)
                {
                    list.Add(archiveEntry);
                }
                else
                {
                    archiveEntry.Length = -1;
                    directories.Add(archiveEntry);
                }
            }

            // NOTE: サイズ0であり、他のエントリ名のパスを含む場合はディレクトリとみなし除外する。
            list = list.Where(entry => entry.Length > 0 || list.All(e => e == entry || !e.EntryName.StartsWith(LoosePath.TrimDirectoryEnd(entry.EntryName)))).ToList();

            // ディレクトリエントリを追加
            list.AddRange(CreateDirectoryEntries(list.Concat(directories)));

            await Task.CompletedTask;
            return list;
        }


        // エントリーのストリームを得る
        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            await Task.CompletedTask; // TODO: async

            lock (_lock)
            {
                var info = entry.Instance as SusieArchiveEntry ?? throw new InvalidCastException();
                var plugin = GetPlugin() ?? throw new SusieException("Cannot found archive plugin");

                byte[] buffer = plugin.ExtractArchiveEntry(Path, info.Position);
                return new MemoryStream(buffer, 0, buffer.Length, false, true);
            }
        }


        // ファイルに出力する
        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string extractFileName, bool isOverwrite, CancellationToken token)
        {
            if (entry.Id < 0) throw new ApplicationException("Cannot open this entry: " + entry.EntryName);

            var info = entry.Instance as SusieArchiveEntry ?? throw new InvalidCastException();
            var plugin = GetPlugin() ?? throw new SusieException("Cannot found archive plugin");

            await Task.CompletedTask; // TODO: async

            // 16MB以上のエントリは直接ファイル出力を試みる
            if (entry.Length > 16 * 1024 * 1024)
            {
                string tempDirectory = Temporary.Current.CreateCountedTempFileName("susie", "");

                try
                {
                    // susieプラグインでは出力ファイル名を指定できないので、
                    // テンポラリフォルダーに出力してから移動する
                    Directory.CreateDirectory(tempDirectory);

                    // 注意：失敗することがよくある
                    plugin.ExtracArchiveEntrytToFolder(Path, info.Position, tempDirectory);

                    // 上書き時は移動前に削除
                    if (isOverwrite && File.Exists(extractFileName))
                    {
                        File.Delete(extractFileName);
                    }

                    var files = Directory.GetFiles(tempDirectory);
                    File.Move(files[0], extractFileName);
                    Directory.Delete(tempDirectory, true);

                    return;
                }

                // 失敗したら：メモリ展開からのファイル保存を行う
                catch (SusieException e)
                {
                    Debug.WriteLine(e.Message);
                }

                // 後始末
                finally
                {
                    if (Directory.Exists(tempDirectory))
                    {
                        Directory.Delete(tempDirectory, true);
                    }
                }
            }

            // メモリ展開からのファイル保存
            {
                byte[] buffer = plugin.ExtractArchiveEntry(Path, info.Position);
                using (var ms = new System.IO.MemoryStream(buffer, false))
                using (var stream = new System.IO.FileStream(extractFileName, System.IO.FileMode.Create))
                {
                    ms.WriteTo(stream);
                }
                GC.Collect();
            }
        }

        /// <summary>
        /// 事前展開？
        /// </summary>
        public override bool CanPreExtract()
        {
            var plugin = GetPlugin();
            return plugin != null && plugin.Plugin.IsPreExtract;
        }
    }
}
