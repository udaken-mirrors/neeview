using NeeLaboratory.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// ドライブサムネイル
    /// </summary>
    public class DriveThumbnail : BindableBase, IThumbnail
    {
        private readonly string _path;
        private ImageSource? _bitmapSource;
        private bool _initialized;

        public DriveThumbnail(string path)
        {
            _path = path;
        }

        public ImageSource? ImageSource => CreateBitmap();
        public double Width => ImageSource is BitmapSource bitmap ? bitmap.PixelWidth : ImageSource != null ? ImageSource.Width : 0.0;
        public double Height => ImageSource is BitmapSource bitmap ? bitmap.PixelHeight : ImageSource != null ? ImageSource.Height : 0.0;
        public bool IsUniqueImage => true;
        public bool IsNormalImage => false;
        public Brush Background => Brushes.Transparent;

        private ImageSource? CreateBitmap()
        {
            if (!_initialized)
            {
                _initialized = true;
                _ = DriveIconUtility.CreateDriveIconAsync(_path,
                    image =>
                    {
                        _bitmapSource = image.GetBitmapSource(256.0);
                        DriveIconUtility.SetDriveIconCache(_path, _bitmapSource);
                        RaisePropertyChanged("");
                    });
            }

            return DriveIconUtility.GetDriveIconCache(_path);
        }
    }
}
