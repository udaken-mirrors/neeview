namespace NeeView
{
    public enum BookSettingSelectMode
    {
        [AliasName]
        Default,

        [AliasName]
        Continue,

        [AliasName]
        RestoreOrDefault,

        [AliasName]
        RestoreOrContinue,

        [AliasName(IsVisibled = false)]
        RestoreOrDefaultReset,
    }

    public enum BookSettingPageSelectMode
    {
        [AliasName]
        Default,

        [AliasName]
        RestoreOrDefault,

        [AliasName]
        RestoreOrDefaultReset,
    }

    public static class BookSettingSelectorForPageExtensions
    {
        public static BookSettingPageSelectMode ToPageSelectMode(this BookSettingSelectMode self)
        {
            return self switch
            {
                BookSettingSelectMode.RestoreOrDefaultReset => BookSettingPageSelectMode.RestoreOrDefaultReset,
                BookSettingSelectMode.RestoreOrDefault or BookSettingSelectMode.RestoreOrContinue => BookSettingPageSelectMode.RestoreOrDefault,
                _ => BookSettingPageSelectMode.Default,
            };
        }

        public static BookSettingSelectMode ToNormalSelectMode(this BookSettingPageSelectMode self)
        {
            return self switch
            {
                BookSettingPageSelectMode.RestoreOrDefaultReset => BookSettingSelectMode.RestoreOrDefaultReset,
                BookSettingPageSelectMode.RestoreOrDefault => BookSettingSelectMode.RestoreOrDefault,
                _ => BookSettingSelectMode.Default,
            };
        }
    }

}
