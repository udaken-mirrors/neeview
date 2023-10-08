using System.Windows;
using System.Windows.Media;

namespace NeeView.PageFrames
{
    public class LoupeTransform
    {
        private double _scale = 1.0;
        private Point _point;
        private readonly ScaleTransform _scaleTransform;
        private readonly TranslateTransform _translateTransform;
        private readonly TransformGroup _transformGroup;


        public LoupeTransform()
        {
            _scaleTransform = new ScaleTransform(1.0, 1.0);
            _translateTransform = new TranslateTransform();
            _transformGroup = new TransformGroup();
            _transformGroup.Children.Add(_translateTransform);
            _transformGroup.Children.Add(_scaleTransform);
        }


        public ScaleTransform ScaleTransform => _scaleTransform;
        public TranslateTransform TranslateTransform => _translateTransform;
        public Transform Transform => _transformGroup;


        public Point Point => _point;
        public double Scale => _scale;


        public void SetScale(double value)
        {
            if (_scale != value)
            {
                _scale = value;
                _scaleTransform.ScaleX = _scale;
                _scaleTransform.ScaleY = _scale;
            }
        }

        public void SetPoint(Point value)
        {
            if (_point != value)
            {
                _point = value;
                _translateTransform.X = _point.X;
                _translateTransform.Y = _point.Y;
            }
        }
    }
}
