namespace NeeLaboratory
{
    public enum AngleDirection
    {
        Forward, Right, Back, Left,
    }

    public static class AngleDirectionExtensions
    {
        public static bool IsVertical(this AngleDirection direction)
        {
            return direction == AngleDirection.Forward || direction == AngleDirection.Back;
        }

        public static bool IsHorizontal(this AngleDirection direction)
        {
            return direction == AngleDirection.Left || direction == AngleDirection.Right;
        }
    }
}

