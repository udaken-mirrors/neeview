using System.Windows.Media;

namespace NeeView.PageFrames
{
    public interface ITransform
    {
        public ScaleTransform FlipTransform { get; }
        public ScaleTransform ScaleTransform { get; }
        public RotateTransform RotateTransform { get; }
        public TranslateTransform TranslateTransform { get; }

        public Transform Transform { get; }
    }
}
