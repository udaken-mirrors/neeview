namespace NeeView
{
    /// <summary>
    /// 回転の初期化モード
    /// </summary>
    public enum AngleResetMode
    {
        /// <summary>
        /// 現在の角度を維持
        /// </summary>
        None,

        /// <summary>
        /// 通常。AutoRotateするかを判定し角度を求める
        /// </summary>
        Normal,

        /// <summary>
        /// AutoRotateの角度を強制適用する
        /// </summary>
        ForceAutoRotate,
    }
}
