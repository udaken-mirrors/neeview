using System.Windows.Media;

namespace NeeView
{
    public interface IHasScalingMode
    {
        BitmapScalingMode? ScalingMode { get; set; }
    }
}
