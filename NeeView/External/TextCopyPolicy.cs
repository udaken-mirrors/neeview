namespace NeeView
{
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
