using System.Windows;
using System.Windows.Media;

namespace NeeView.PageFrames
{
    public class NormalTransform : ITransform
    {

        private readonly ScaleTransform _flipTransform;
        private readonly ScaleTransform _scaleTransform;
        private readonly RotateTransform _rotateTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TransformGroup _transformGroup;


        public NormalTransform()
        {
            _flipTransform = new ScaleTransform();
            _scaleTransform = new ScaleTransform();
            _rotateTransform = new RotateTransform();
            _translateTransform = new TranslateTransform();
            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_flipTransform);
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_rotateTransform);
            _transformGroup.Children.Add(_translateTransform);
        }


        public Transform Transform => _transformGroup;
        public ScaleTransform FlipTransform => _flipTransform;
        public ScaleTransform ScaleTransform => _scaleTransform;
        public RotateTransform RotateTransform => _rotateTransform;
        public TranslateTransform TranslateTransform => _translateTransform;


        public void SetScale(double value)
        {
            _scaleTransform.ScaleX = value;
            _scaleTransform.ScaleY = value;
        }

        public void SetAngle(double value)
        {
            _rotateTransform.Angle = value;
        }

        public void SetPoint(Point value)
        {
            _translateTransform.X = value.X;
            _translateTransform.Y = value.Y;
        }

        public void SetFlipHorizontal(bool isFlipHorizontal)
        {
            _flipTransform.ScaleX = isFlipHorizontal ? -1.0 : 1.0;
        }

        public void SetFlipVertical(bool isFlipVertical)
        {
            _flipTransform.ScaleY = isFlipVertical ? -1.0 : 1.0;
        }
    }
}
