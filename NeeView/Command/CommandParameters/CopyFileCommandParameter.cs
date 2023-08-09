using NeeView.Windows.Property;

namespace NeeView
{
    /// <summary>
    /// CopyFileCommand Parameter
    /// </summary>
    public class CopyFileCommandParameter : CommandParameter 
    {
        private ArchivePolicy _archivePolicy = ArchivePolicy.SendExtractFile;
        private MultiPagePolicy _multiPagePolicy = MultiPagePolicy.Once;
        private TextCopyPolicy _textCopyPolicy = TextCopyPolicy.None;


        // 複数ページのときの動作
        [PropertyMember]
        public MultiPagePolicy MultiPagePolicy
        {
            get { return _multiPagePolicy; }
            set { SetProperty(ref _multiPagePolicy, value); }
        }

        // 圧縮ファイルのときの動作
        [PropertyMember]
        public ArchivePolicy ArchivePolicy
        {
            get { return _archivePolicy; }
            set { SetProperty(ref _archivePolicy, value); }
        }

        // テキストコピーのヒント
        [PropertyMember]
        public TextCopyPolicy TextCopyPolicy
        {
            get { return _textCopyPolicy; }
            set { SetProperty(ref _textCopyPolicy, value); }
        }
    }


    /// <summary>
    /// テキストコピーのヒント
    /// </summary>
    public enum TextCopyPolicy
    {
        /// <summary>
        /// テキストにしない
        /// </summary>
        None,

        /// <summary>
        /// コピーファイルの実体パス
        /// </summary>
        CopyFilePath,

        /// <summary>
        /// 元のパス
        /// </summary>
        OriginalPath,
    }
}
