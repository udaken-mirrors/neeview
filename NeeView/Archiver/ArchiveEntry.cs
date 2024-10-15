using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイブエントリ
    /// </summary>
    public partial class ArchiveEntry : IRenameable, IDisposable
    {
        /// <summary>
        /// Emptyインスタンス
        /// </summary>
        public static ArchiveEntry Empty { get; } = new ArchiveEntry(StaticFolderArchive.Default) { IsEmpty = true };

        private string _rawEntryName = "";
        private FileProxy? _fileProxy;
        public object? _data;
        private PreExtractMemory.Key? _preExtractMemoryKey;
        private bool _disposedValue;
        private readonly object _lock = new();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="archiver">所属アーカイバ</param>
        public ArchiveEntry(Archiver archiver)
        {
            Archiver = archiver;
        }


        [Subscribable]
        public event EventHandler? DataChanged;


        /// <summary>
        ///  Emptyインスタンス？
        /// </summary>
        public bool IsEmpty { get; init; }

        /// <summary>
        /// 仮のエントリー
        /// </summary>
        public bool IsTemporary { get; init; }

        /// <summary>
        /// 所属アーカイバ
        /// </summary>
        public Archiver Archiver { get; private set; }

        /// <summary>
        /// アーカイブ内登録番号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// エントリ情報
        /// アーカイバーで識別子として使用される
        /// </summary>
        public object? Instance { get; set; }

        /// <summary>
        /// エントリデータ。
        /// 先読みデータ。テンポラリファイル名もしくはバイナリデータ
        /// </summary>
        public object? Data => _data;

        /// <summary>
        /// エントリデータ存在？
        /// </summary>
        public bool HasCache => Data is not null;

        /// <summary>
        /// パスが有効であるか
        /// 無効である場合はアーカイブパスである可能性あり
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// アーカイブパスであるか
        /// </summary>
        public bool IsArchivePath { get; private set; }

        // 例：
        // a.zip 
        // +- b.zip
        //      +- c\001.jpg <- this!

        /// <summary>
        /// エントリ名(重複有)
        /// </summary>
        /// c\001.jpg
        public string RawEntryName
        {
            get { return _rawEntryName; }
            set
            {
                if (_rawEntryName != value)
                {
                    _rawEntryName = value;
                    this.EntryName = NormalizeEntryName(_rawEntryName);
                }
            }
        }

        /// <summary>
        /// エントリ名(重複有、正規化済)
        /// </summary>
        /// c/001.jpg => c\001.jpg
        public string EntryName { get; private set; } = "";

        /// <summary>
        /// エントリのターゲットパス。<br/>
        /// プレイリストの場合の参照先パスになる(アーカイブパスの可能性あり)
        /// </summary>
        public string? Target { get; set; }

        /// <summary>
        /// ショートカットの場合のリンク先パス。<br/>
        /// プレイリストの場合はリンクを解決した参照先パスになる
        /// </summary>
        public string? Link { get; set; }

        /// <summary>
        /// エントリ名のファイル名
        /// </summary>
        /// 001.jpg
        public string EntryLastName => LoosePath.GetFileName(EntryName);

        /// <summary>
        /// エントリのフルネーム
        /// </summary>
        public string EntryFullName => LoosePath.Combine(Archiver.SystemPath, EntryName);

        /// <summary>
        /// ルートアーカイバー
        /// </summary>
        /// a.zip
        // TODO: ArchiveEntry.RootArchiver is not null
        public Archiver RootArchiver => Archiver.RootArchiver;

        /// <summary>
        /// 所属名
        /// </summary>
        public string RootArchiverName => RootArchiver.EntryName;

        /// <summary>
        /// エクスプローラーから指定可能なパス
        /// </summary>
        public string SystemPath
        {
            get
            {
                if (Link != null)
                {
                    return Link;
                }
                else
                {
                    return EntryFullName;
                }
            }
        }

        /// <summary>
        /// 識別名
        /// アーカイブ内では重複名があるので登録番号を含めたユニークな名前にする
        /// </summary>
        public string Ident => LoosePath.Combine(Archiver.Ident, $"{Id}.{EntryName}");

        /// <summary>
        /// ファイルサイズ。
        /// -1 はディレクトリ
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// ディレクトリ？
        /// </summary>
        public bool IsDirectory => Length == -1;

#if false
        /// <summary>
        /// ディレクトリは空であるか
        /// </summary>
        public bool IsEmpty { get; set; }
#endif

        /// <summary>
        /// ファイル作成日
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// ファイル更新日
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// ファイルシステム所属判定
        /// </summary>
        public bool IsFileSystem => Archiver.IsFileSystemEntry(this);

        /// <summary>
        /// 削除済フラグ
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// ショートカット判定
        /// </summary>
        /// <remarks>
        /// プレイリスト自体はショートカット扱いとする。
        /// プレイリスト項目はショートカットとみなさない。
        /// </remarks>
        public bool IsShortcut => PlaylistArchive.IsSupportExtension(EntryName) || (Archiver is not PlaylistArchive && Link != null);

        /// <summary>
        /// エントリ名の正規化
        /// </summary>
        public static string NormalizeEntryName(string rawEntryName)
        {
            return LoosePath.TrimEnd(LoosePath.NormalizeSeparator(rawEntryName));
        }

        /// <summary>
        /// エントリデータ設定
        /// </summary>
        /// <param name="value"></param>
        public void SetData(object value)
        {
            if (_disposedValue) return;

            lock (_lock)
            {
                if (_data == value) return;
                ResetData();
                if (value is null) return;
                _data = value;
                if (value is byte[] rawData)
                {
                    _preExtractMemoryKey = PreExtractMemory.Current.Open(rawData.Length);
                }
            }
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// エントリーデータリセット
        /// </summary>
        public void ResetData()
        {
            lock (_lock)
            {
                if (_data is null) return;
                _preExtractMemoryKey?.Dispose();
                _preExtractMemoryKey = null;
                _data = null;
            }
        }

        /// <summary>
        /// ファイルシステムでのパスを返す
        /// </summary>
        /// <returns>パス。圧縮ファイルの場合は null</returns>
        public string? GetFileSystemPath()
        {
            return Archiver.GetFileSystemPath(this);
        }

        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <returns>Stream</returns>
        public async Task<Stream> OpenEntryAsync(CancellationToken token)
        {
            return await Archiver.OpenStreamAsync(this, token);
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="exportFileName">出力ファイル名</param>
        /// <param name="isOverwrite">上書き許可フラグ</param>
        public async Task ExtractToFileAsync(string exportFileName, bool isOverwrite, CancellationToken token)
        {
            if (exportFileName is null) throw new ArgumentNullException(nameof(exportFileName));

            await Archiver.ExtractToFileAsync(this, exportFileName, isOverwrite, token);
        }


        /// <summary>
        /// テンポラリにアーカイブエントリを解凍する
        /// このテンポラリは自動的に削除される
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="isKeepFileName">エントリー名をファイル名にする</param>
        public async Task<FileProxy> GetFileProxyAsync(bool isKeepFileName, CancellationToken token)
        {
            _fileProxy = _fileProxy ?? await CreateFileProxyAsync(new TempArchiveEntryNamePolicy(isKeepFileName, "entry"), false, token);
            return _fileProxy;
        }

        /// <summary>
        /// テンポラリアーカイブエントリを得る。
        /// 場合によっては存在するファイルをそのまま返す。
        /// </summary>
        /// <param name="fileNamePolicy">ファイル名ポリシ―</param>
        /// <param name="isOverwrite">上書き許可</param>
        /// <returns>ファイル</returns>
        public async Task<FileProxy> CreateFileProxyAsync(TempArchiveEntryNamePolicy fileNamePolicy, bool isOverwrite, CancellationToken token)
        {
            var targetPath = Link ?? GetFileSystemPath();
            if (targetPath is not null && (this.Archiver is FolderArchive || this.Archiver is MediaArchiver || IsFileSystem))
            {
                return new FileProxy(targetPath);
            }

            await WaitPreExtractAsync(token);

            var tempFileName = fileNamePolicy.Create(this);

            if (this.Data is string fileName)
            {
                if (fileNamePolicy.IsKeepFileName && fileName != tempFileName)
                {
                    await FileIO.CopyFileAsync(fileName, tempFileName, isOverwrite, token);
                    return new TempFile(fileName);
                }
                else
                {
                    return new FileProxy(fileName);
                }
            }
            else if (this.Data is byte[] rawData)
            {
                FileIO.CheckOverwrite(tempFileName, isOverwrite);
                await File.WriteAllBytesAsync(tempFileName, rawData, token);
                return new TempFile(tempFileName);
            }
            else
            {
                await ExtractToFileAsync(tempFileName, isOverwrite, token);
                return new TempFile(tempFileName);
            }
        }

        /// <summary>
        /// エントリ単位で事前展開完了を待機する
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitPreExtractAsync(CancellationToken token)
        {
            await Archiver.WaitPreExtractAsync(this, token);
        }

        /// <summary>
        /// このエントリがブックであるかを判定。
        /// アーカイブのほかメディアを含める
        /// </summary>
        public bool IsBook()
        {
            if (this.IsDirectory)
            {
                return true;
            }

            return ArchiverManager.Current.IsSupported(this.Link ?? this.EntryName, false, true);
        }

        /// <summary>
        /// このエントリがアーカイブであるかを判定。
        /// メディアは除外する
        /// </summary>
        public bool IsArchive()
        {
            if (this.IsDirectory)
            {
                return this.IsFileSystem; // アーカイブディレクトリは除外
            }

            return ArchiverManager.Current.IsSupported(this.Link ?? this.EntryName, false, false);
        }

        /// <summary>
        /// アーカイブ中のディレクトリ？
        /// </summary>
        public bool IsArchiveDirectory()
        {
            return !IsFileSystem && IsDirectory;
        }

        /// <summary>
        /// メディア？
        /// </summary>
        public bool IsMedia()
        {
            return !this.IsDirectory && ArchiverManager.Current.GetSupportedType(this.Link ?? this.EntryLastName) == ArchiverType.MediaArchiver;
        }

        /// <summary>
        /// このエントリが画像であるか拡張子から判定。
        /// MediaArchiverは無条件で画像と認識
        /// </summary>
        public bool IsImage(bool includeMedia = true)
        {
            return !this.IsDirectory && ((this.Archiver is MediaArchiver) || PictureProfile.Current.IsSupported(this.Link ?? this.EntryName, includeMedia));
        }

        /// <summary>
        /// exists?
        /// </summary>
        public bool Exists()
        {
            if (IsDeleted) return false;
            return Archiver.Exists(this);
        }

        /// <summary>
        /// can delete?
        /// </summary>
        public bool CanDelete()
        {
            return Archiver.CanDelete(this);
        }

        /// <summary>
        /// delete
        /// </summary>
        public async Task<bool> DeleteAsync()
        {
            return await Archiver.DeleteAsync(this);
        }

        /// <summary>
        /// 複数エントリをまとめて削除
        /// </summary>
        public static async Task<bool> DeleteEntriesAsync(IEnumerable<ArchiveEntry> entries)
        {
            if (!entries.Any()) return false;

            foreach (var group in entries.GroupBy(e => e.Archiver))
            {
                var archiver = group.Key;
                archiver.ClearEntryCache();
                var isSuccess = await archiver.DeleteAsync(group.ToList());
                if (!isSuccess) return false;
            }

            return true;
        }

        /// <summary>
        /// to string
        /// </summary>
        public override string? ToString()
        {
            return string.IsNullOrEmpty(EntryName) ? base.ToString() : $"{Id}:{EntryName}";
        }


        public string GetRenameText()
        {
            return EntryLastName;
        }

        public bool CanRename()
        {
            return Archiver.CanRename(this);
        }

        public async Task<bool> RenameAsync(string name)
        {
            return await Archiver.RenameAsync(this, name);
        }

        /// <summary>
        /// エントリの実体化は可能か？
        /// </summary>
        public bool CanRealize()
        {
            return this.IsFileSystem || !this.IsArchiveDirectory();
        }

        /// <summary>
        /// エントリを実体ファイルにする
        /// </summary>
        /// <param name="token"></param>
        /// <returns>実体ファイルのパス</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        public async Task<string?> RealizeAsync(CancellationToken token)
        {
            return await RealizeAsync(ArchivePolicy.SendExtractFile, token);
        }

        /// <summary>
        /// エントリを実体ファイルにする
        /// </summary>
        /// <param name="archivePolicy">アーカイブファイルのときの方針</param>
        /// <param name="token"></param>
        /// <returns>実体ファイルのパス。取得できなかったときは null</returns>
        /// <exception cref="NotSupportedException">サポートされていない ArchivePolicy</exception>
        public async Task<string?> RealizeAsync(ArchivePolicy archivePolicy, CancellationToken token)
        {
            // file
            if (this.IsFileSystem)
            {
                return this.GetFileSystemPath() ?? throw new InvalidOperationException();
            }

            // playlist item
            if (this.Archiver is PlaylistArchive && this.Instance is ArchiveEntry archiveEntry)
            {
                return await archiveEntry.RealizeAsync(archivePolicy, token);
            }

            // in archive
            switch (archivePolicy)
            {
                case ArchivePolicy.SendArchiveFile:
                    return this.GetFileSystemPath() ?? this.Archiver.GetPlace();

                case ArchivePolicy.SendExtractFile:
                    if (!this.IsArchiveDirectory())
                    {
                        var proxy = await this.GetFileProxyAsync(true, token);
                        return proxy.Path;
                    }
                    else
                    {
                        // TODO: ArchiveDirectory 対応
                        return null;
                    }

                case ArchivePolicy.SendArchivePath:
                    return this.EntryFullName;

                default:
                    throw new NotSupportedException($"Unsupported archive policy: {archivePolicy}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _preExtractMemoryKey?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}

