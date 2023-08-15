namespace NeeView
{
    // 自動回転タイプ
    public enum AutoRotateType
    {
        [AliasName]
        None,

        [AliasName]
        Left,

        [AliasName]
        Right,
    }

    public static class AutoRotateTypeExtensions
    {
        public static double ToAngle(this AutoRotateType self)
        {
            return self switch
            {
                AutoRotateType.Left => -90.0,
                AutoRotateType.Right => 90.0,
                _ => 0.0,
            };
        }
    }

}
