using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// アーカイブエントリ
    /// </summary>
    public class ArchiveEntry
    {
        /// <summary>
        /// Emptyインスタンス
        /// </summary>
        public static ArchiveEntry Empty { get; } = new ArchiveEntry(StaticFolderArchive.Default) { IsEmpty = true };


        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="archiver">所属アーカイバ</param>
        public ArchiveEntry(Archiver archiver)
        {
            Archiver = archiver;
        }


        /// <summary>
        ///  Emptyインスタンス？
        /// </summary>
        public bool IsEmpty { get; private set; }


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
        public object? Data { get; set; }

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
        private string _rawEntryName = "";
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
        /// ショートカットの場合のリンク先パス。<br/>
        /// ページマークの場合の参照先パスにもなる(アーカイブパスの可能性あり)
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
        /// 拡張子による画像ファイル判定無効
        /// </summary>
        public bool IsIgnoreFileExtension { get; set; }

        /// <summary>
        /// 削除済フラグ
        /// </summary>
        public bool IsDeleted { get; set; }


        /// <summary>
        /// エントリ名の正規化
        /// </summary>
        public static string NormalizeEntryName(string rawEntryName)
        {
            return LoosePath.TrimEnd(LoosePath.NormalizeSeparator(rawEntryName));
        }

        /// <summary>
        /// エントリデータを先読みデータとして返す
        /// </summary>
        /// <returns></returns>
        public byte[]? GetRawData()
        {
            return Data as byte[];
        }

        /// <summary>
        /// ファイルシステムでのパスを返す
        /// </summary>
        /// <returns>パス。圧縮ファイルの場合はnull</returns>
        public string? GetFileSystemPath()
        {
            return Archiver.GetFileSystemPath(this);
        }

        /// <summary>
        /// ストリームを開く
        /// </summary>
        /// <returns>Stream</returns>
        public Stream OpenEntry()
        {
            return Archiver.OpenStream(this);
        }

        /// <summary>
        /// ファイルに出力する
        /// </summary>
        /// <param name="exportFileName">出力ファイル名</param>
        /// <param name="isOverwrite">上書き許可フラグ</param>
        public void ExtractToFile(string exportFileName, bool isOverwrite)
        {
            if (exportFileName is null) throw new ArgumentNullException(nameof(exportFileName));

            Archiver.ExtractToFile(this, exportFileName, isOverwrite);
        }


        /// <summary>
        /// テンポラリにアーカイブを解凍する
        /// このテンポラリは自動的に削除される
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="isKeepFileName">エントリー名をファイル名にする</param>
        public FileProxy ExtractToTemp(bool isKeepFileName = false)
        {
            var targetPath = Link ?? GetFileSystemPath();
            if (targetPath is not null && (this.Archiver is FolderArchive || this.Archiver is MediaArchiver || IsFileSystem))
            {
                return new FileProxy(targetPath);
            }
            else
            {
                string tempFileName = isKeepFileName
                    ? Temporary.Current.CreateTempFileName(LoosePath.GetFileName(EntryName))
                    : Temporary.Current.CreateCountedTempFileName("entry", System.IO.Path.GetExtension(EntryName));
                ExtractToFile(tempFileName, false);
                return new TempFile(tempFileName);
            }
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

            return ArchiverManager.Current.IsSupported(EntryName, false, true);
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

            return ArchiverManager.Current.IsSupported(EntryName, false, false);
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
            return !this.IsDirectory && ArchiverManager.Current.GetSupportedType(this.EntryLastName) == ArchiverType.MediaArchiver;
        }

        /// <summary>
        /// このエントリが画像であるか拡張子から判定。
        /// MediaArchiverは無条件で画像と認識
        /// </summary>
        public bool IsImage()
        {
            return !this.IsDirectory && ((this.Archiver is MediaArchiver) || PictureProfile.Current.IsSupported(this.Link ?? this.EntryName));
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
            return string.IsNullOrEmpty(EntryName) ? base.ToString() : EntryName;
        }
    }
}

