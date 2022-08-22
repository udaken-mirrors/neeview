using SevenZip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイバー：7z.dll
    /// </summary>
    public class SevenZipArchiver : Archiver, IDisposable
    {
        private static object _staticLock = new object();

        private SevenZipAccessor _accessor;
        private string? _format;


        public SevenZipArchiver(string path, ArchiveEntry? source) : base(path, source)
        {
            _accessor = new SevenZipAccessor(Path);
        }


        public override string ToString()
        {
            return "7-Zip" + (_format != null ? $" ({_format})" : null);
        }

        public override void Unlock()
        {
            // 直接の圧縮ファイルである場合のみアンロック
            if (this.Parent == null || this.Parent is FolderArchive)
            {
                _accessor.Unlock();
            }
        }


        // サポート判定
        public override bool IsSupported()
        {
            return true;
        }

        // Solid archive ?
        private bool IsSolid()
        {
            ThrowIfDisposed();

            return _accessor.IsSolid;
        }

        // エントリーリストを得る
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            ReadOnlyCollection<ArchiveFileInfo> entries;

            // NOTE: 異なるスレッドで処理するととても重くなることがあるので排他処理にする
            lock (_staticLock)
            {
                _format = _accessor.Format;
                entries = _accessor.ArchiveFileData;
            }

            for (int id = 0; id < entries.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = entries[id];

                var archiveEntry = new ArchiveEntry()
                {
                    IsValid = true,
                    Archiver = this,
                    Id = id,
                    RawEntryName = entry.FileName,
                    Length = (long)entry.Size,
                    LastWriteTime = entry.LastWriteTime,
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

            // ディレクトリエントリを追加
            list.AddRange(CreateDirectoryEntries(list.Concat(directories)));

            await Task.CompletedTask;
            return list;
        }

        // エントリーのストリームを得る
        protected override Stream OpenStreamInner(ArchiveEntry entry)
        {
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);

            ThrowIfDisposed();

            var archiveEntry = _accessor.ArchiveFileData[entry.Id];
            if (archiveEntry.FileName != entry.RawEntryName)
            {
                throw new ApplicationException(Properties.Resources.InconsistencyException_Message);
            }

            var ms = new MemoryStream();
            _accessor.ExtractFile(entry.Id, ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        // ファイルに出力
        protected override void ExtractToFileInner(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);

            ThrowIfDisposed();

            using (Stream fs = new FileStream(exportFileName, FileMode.Create, FileAccess.Write))
            {
                _accessor.ExtractFile(entry.Id, fs);
            }
        }

        /// <summary>
        /// 事前展開？
        /// </summary>
        public override async Task<bool> CanPreExtractAsync(CancellationToken token)
        {
            ThrowIfDisposed();

            if (!IsSolid()) return false;

            var entries = await GetEntriesAsync(token);
            var extractSize = entries.Select(e => e.Length).Sum();
            return extractSize / (1024 * 1024) < Config.Current.Performance.PreExtractSolidSize;
        }

        /// <summary>
        /// 事前展開処理
        /// </summary>
        public override async Task PreExtractInnerAsync(string directory, CancellationToken token)
        {
            ThrowIfDisposed();

            if (Config.Current.Performance.IsPreExtractToMemory)
            {
                await PreExtractMemoryAsync(token);
            }
            else
            {
                await PreExtractTempFileAsync(directory, token);
            }
        }

        private async Task PreExtractTempFileAsync(string directory, CancellationToken token)
        {
            ThrowIfDisposed();

            var entries = await GetEntriesAsync(token);

            using (var extractor = new SevenZipExtractor(this.Path))
            {
                var tempExtractor = new SevenZipTempFileExtractor();
                tempExtractor.TempFileExtractionFinished += Temp_TempFileExtractionFinished;
                tempExtractor.ExtractArchive(extractor, directory);
            }

            void Temp_TempFileExtractionFinished(object? sender, SevenZipTempFileExtractionArgs e)
            {
                var entry = entries.FirstOrDefault(a => a.Id == e.FileInfo.Index);
                if (entry != null)
                {
                    entry.Data = e.FileName;
                }
            }
        }

        private async Task PreExtractMemoryAsync(CancellationToken token)
        {
            ThrowIfDisposed();

            var entries = await GetEntriesAsync(token);

            using (var extractor = new SevenZipExtractor(this.Path))
            {
                var tempExtractor = new SevenZipMemoryExtractor();
                tempExtractor.TempFileExtractionFinished += Temp_TempFileExtractionFinished;
                tempExtractor.ExtractArchive(extractor);
            }

            void Temp_TempFileExtractionFinished(object? sender, SevenZipMemoryExtractionArgs e)
            {
                var entry = entries.FirstOrDefault(a => a.Id == e.FileInfo.Index);
                if (entry != null)
                {
                    entry.Data = e.RawData;
                }
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _accessor.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

}
