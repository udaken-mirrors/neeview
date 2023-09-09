namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame更新要求レベル
    /// </summary>
    public enum PageFrameDirtyLevel
    {
        /// <summary>
        /// 変更なし
        /// </summary>
        Clean,

        /// <summary>
        /// 変更の可能性あり。
        /// PageFrame情報に差異があればViewContent更新
        /// </summary>
        Moderate,

        /// <summary>
        /// 変更。
        /// PageFrame情報に差異がなくてもViewContent更新。
        /// フィルター等のPageFrame以外の情報変更による。
        /// </summary>
        Heavy,

        /// <summary>
        /// コンテナから作り直し
        /// </summary>
        Replace,
    }

}


