using System.IO;

namespace NeeView
{
    /// <summary>
    /// 一時ファイル名ポリシー
    /// </summary>
    public class TempArchiveEntryNamePolicy
    {
        /// <summary>
        /// アーカイブ展開時の一時ファイル名ポリシー
        /// </summary>
        /// <param name="isKeepFileName">ファイル名を維持する</param>
        /// <param name="prefix">自動生成時の名前のプレフィックス</param>
        public TempArchiveEntryNamePolicy(bool isKeepFileName, string prefix)
        {
            IsKeepFileName = isKeepFileName;
            Prefix = prefix;
        }

        public bool IsKeepFileName { get; }
        public string Prefix { get; }

        public string Create(ArchiveEntry entry)
        {
            if (IsKeepFileName)
            {
                return Temporary.Current.CreateTempFileName(LoosePath.GetFileName(entry.EntryName)); ;
            }
            else
            {
                return Temporary.Current.CreateCountedTempFileName(Prefix, Path.GetExtension(entry.EntryName));
            }
        }
    }

}

