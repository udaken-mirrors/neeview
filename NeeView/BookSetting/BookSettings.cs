namespace NeeView
{
    public static class BookSettings
    {
        /// <summary>
        /// 現在のブック設定
        /// </summary>
        public static BookSettingAccessor Current { get; } = new BookSettingAccessor(Config.Current.BookSetting);

        /// <summary>
        /// 既定のブック設定 (未使用)
        /// </summary>
        //public static BookSettingAccessor Default { get; } = new BookSettingAccessor(Config.Current.BookSettingDefault);
    }

}
