using System.Windows;

namespace NeeView
{
    public static class PointExtensions
    {
        // 開発用：座標表示
        public static string ToIntString(this Point point)
        {
            return $"({(int)point.X},{(int)point.Y})";
        }
    }


}