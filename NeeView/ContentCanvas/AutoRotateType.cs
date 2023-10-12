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

        [AliasName]
        ForcedLeft,

        [AliasName]
        ForcedRight,
    }

    public static class AutoRotateTypeExtensions
    {
        public static double ToAngle(this AutoRotateType self)
        {
            return self switch
            {
                AutoRotateType.Left => -90.0,
                AutoRotateType.Right => 90.0,
                AutoRotateType.ForcedLeft => -90.0,
                AutoRotateType.ForcedRight => 90.0,
                _ => 0.0,
            };
        }

        public static bool IsForced(this AutoRotateType self)
        {
            return self switch
            {
                AutoRotateType.ForcedLeft => true,
                AutoRotateType.ForcedRight => true,
                _ => false
            };
        }
    }

}
