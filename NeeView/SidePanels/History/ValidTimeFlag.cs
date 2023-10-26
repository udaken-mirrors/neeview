namespace NeeView
{
    /// <summary>
    /// 発行とその時間。リクエスト要求等に使用される。
    /// 寿命付きフラグ
    /// </summary>
    public class ValidTimeFlag
    {
        public static ValidTimeFlag Empty => new(false, 0);

        public ValidTimeFlag(bool isEnabled, int timestamp)
        {
            IsEnabled = isEnabled;
            Timestamp = timestamp;
        }

        public bool IsEnabled { get; }
        public int Timestamp { get; }

        public static ValidTimeFlag Create()
        {
            return new ValidTimeFlag(true, System.Environment.TickCount);
        }

        /// <summary>
        /// 有効判定
        /// </summary>
        /// <param name="ms">有効期間</param>
        /// <returns>有効/無効</returns>
        public bool Condition(int ms) => IsEnabled && System.Environment.TickCount - Timestamp < ms;
    }

}
