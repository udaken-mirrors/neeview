namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame更新要求レベル
    /// </summary>
    public enum PageFrameDartyLevel
    {
        /// <summary>
        /// 変更なし
        /// </summary>
        Clean,

        /// <summary>
        /// 変更あり
        /// </summary>
        Moderate,

        /// <summary>
        /// 強めの変更あり。
        /// フィルター等PageFrame情報以外の変更。ViewContent強制更新
        /// </summary>
        Heavy,

        /// <summary>
        /// 作り直し
        /// </summary>
        Replace,
    }

}


