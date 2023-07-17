namespace NeeView
{
    public enum LongButtonMask
    {
        [AliasName]
        Left,
        [AliasName]
        Right,
        [AliasName]
        All,
    }

    public static class LingButtonMasExtensions
    {
        public static MouseButtonBits ToMouseButtonBits(this LongButtonMask self)
        {
            return self switch
            {
                LongButtonMask.Right => MouseButtonBits.RightButton,
                LongButtonMask.All => MouseButtonBits.All,
                _ => MouseButtonBits.LeftButton,
            };
        }
    }
}
