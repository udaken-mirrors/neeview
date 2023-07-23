using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// View生成用パラメータ
    /// </summary>
    public class ViewContentParameters
    {
        public ViewContentParameters(Binding foregroundBrush, Binding pageBackgroundBrush, Binding bitmapScalingMode, Binding animationImageVisibility, Binding animationPlayerVisibility)
        {
            ForegroundBrush = foregroundBrush;
            PageBackgroundBrush = pageBackgroundBrush;
            BitmapScalingMode = bitmapScalingMode;
            AnimationImageVisibility = animationImageVisibility;
            AnimationPlayerVisibility = animationPlayerVisibility;
        }

        public Binding ForegroundBrush { get; set; }
        public Binding PageBackgroundBrush { get; set; }
        public Binding BitmapScalingMode { get; set; }
        public Binding AnimationImageVisibility { get; set; }
        public Binding AnimationPlayerVisibility { get; set; }
    }
}
