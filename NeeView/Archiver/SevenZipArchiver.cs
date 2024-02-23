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
    // TODO: [.] 事前展開の非同期化。エントリ単位で事前展開完了を待機できるようにする。
    // TODO: [.] 事前展開のキャンセル対応
    // TODO: [v] 事前展開メモリサイズに応じてメモリ展開とファイル展開をエントリ単位で切り替える。
    // TODO: [.] SolidArchive の場合は _accessor.ExtractFile() ではなく事前展開を実行し待機する。
    // TODO: [v] メディアファイルはファイル展開
    // TODO: [x] メディア判定とかアーカイブ判定とかはページの種類判定と同じものにせよ
    // TODO: Archiver.Dispose() を呼び出す、もしくは使用停止した時点で PreExtract をキャンセルする
    // TODO: ArchiveEntry の Dispose()
    // TODO: アーカイブをストリームソース対応にして、アーカイブの事前展開先をオンメモリ可能にする。
    // TODO: Solid判定の非同期化
    // TODO: 圧縮サブフォルダーの動作確認
    // TODO: ArchiveEntry.OpenEntryAsync() 対応
    // TODO: SusieArchiver の TODO:Async

    /// <summary>
    /// アーカイバー：7z.dll
    /// </summary>
    public class SevenZipArchiver : Archiver
    {
        private static readonly object _staticLock = new();

        private readonly SevenZipAccessor _accessor;
        private string? _format;
        private bool? _isSolid;


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
                NVDebug.AssertMTA();
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
            if (_isSolid.HasValue)
            {
                return _isSolid.Value;
            }
            else
            {
                NVDebug.AssertMTA();
                return _accessor.IsSolid;
            }
        }

        // エントリーリストを得る
        protected override async Task<List<ArchiveEntry>> GetEntriesInnerAsync(CancellationToken token)
        {
            NVDebug.AssertMTA();
            token.ThrowIfCancellationRequested();

            if (_disposedValue) return new List<ArchiveEntry>();

            var list = new List<ArchiveEntry>();
            var directories = new List<ArchiveEntry>();

            ReadOnlyCollection<ArchiveFileInfo> entries;

            // NOTE: 異なるスレッドで処理するととても重くなることがあるので排他処理にする
            // TODO: アーカイブ初期化でエントリ生成しているが、キャンセルできるようにならないか？
            lock (_staticLock)
            {
                _format = _accessor.Format;
                _isSolid = _accessor.IsSolid;
                entries = _accessor.ArchiveFileData;
            }

            for (int id = 0; id < entries.Count; ++id)
            {
                token.ThrowIfCancellationRequested();

                var entry = entries[id];

                var archiveEntry = new ArchiveEntry(this)
                {
                    IsValid = true,
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
        protected override async Task<Stream> OpenStreamInnerAsync(ArchiveEntry entry, CancellationToken token)
        {
            NVDebug.AssertMTA();
            Debug.Assert(entry is not null);
            Debug.Assert(!IsSolid(), "Pre-extract, so no direct extract.");
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);

            ThrowIfDisposed();

            var archiveEntry = _accessor.ArchiveFileData[entry.Id];
            if (archiveEntry.FileName != entry.RawEntryName)
            {
                throw new ApplicationException(Properties.TextResources.GetString("InconsistencyException.Message"));
            }

            var ms = new MemoryStream();
            token.ThrowIfCancellationRequested();
            await Task.Run(() => _accessor.ExtractFile(entry.Id, ms));
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        // ファイルに出力
        protected override async Task ExtractToFileInnerAsync(ArchiveEntry entry, string exportFileName, bool isOverwrite, CancellationToken token)
        {
            // NOTE: MTAスレッドで実行。SevenZipSharpのCOM例外対策
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                await Task.Run(() => ExtractToFileInnerCore(entry, exportFileName, isOverwrite));
            }
            else
            {
                ExtractToFileInnerCore(entry, exportFileName, isOverwrite);
            }
        }

        private void ExtractToFileInnerCore(ArchiveEntry entry, string exportFileName, bool isOverwrite)
        {
            NVDebug.AssertMTA();
            Debug.Assert(entry is not null);
            Debug.Assert(!string.IsNullOrEmpty(exportFileName));
            Debug.Assert(!IsSolid(), "Pre-extract, so no direct extract.");
            if (entry.Id < 0) throw new ArgumentException("Cannot open this entry: " + entry.EntryName);

            if (_disposedValue) return;

            var mode = isOverwrite ? FileMode.Create : FileMode.CreateNew;
            using (Stream fs = new FileStream(exportFileName, mode, FileAccess.Write))
            {
                _accessor.ExtractFile(entry.Id, fs);
            }
        }

        /// <summary>
        /// 事前展開？
        /// </summary>
        public override bool CanPreExtract()
        {
            if (_disposedValue) return false;
            return IsSolid();
        }

        /// <summary>
        /// 事前展開処理
        /// </summary>
        public override async Task PreExtractAsync(string directory, CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrEmpty(directory));

            if (_disposedValue) return;

            await PreExtractHybridAsync(directory, token);
        }


        private async Task PreExtractHybridAsync(string directory, CancellationToken token)
        {
            Debug.Assert(!string.IsNullOrEmpty(directory));

            if (_disposedValue) return;

            var entries = await GetEntriesAsync(token);
            token.ThrowIfCancellationRequested();

            using (var extractor = new SevenZipExtractor(this.Path))
            {
                var tempExtractor = new SevenZipHybridExtractor(extractor, directory);
                tempExtractor.TempFileExtractionFinished += Temp_TempFileExtractionFinished;
                await tempExtractor.ExtractAsync(token);
                token.ThrowIfCancellationRequested();
            }

            void Temp_TempFileExtractionFinished(object? sender, SevenZipEntry e)
            {
                var entry = entries.FirstOrDefault(a => a.Id == e.FileInfo.Index);
                if (entry != null)
                {
                    entry.SetData(e.Data);
                }
            }
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _accessor.Dispose();
                _disposedValue = true;
                base.Dispose(disposing);
            }
        }
        #endregion
    }

}
