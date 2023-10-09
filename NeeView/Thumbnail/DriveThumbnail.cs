using NeeLaboratory.ComponentModel;
using System;
using System.Windows.Input;
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


        public event EventHandler? Changed;


        public bool IsValid => true;
        public bool IsUniqueImage => true;
        public bool IsNormalImage => false;
        public Brush Background => Brushes.Transparent;


        public ImageSource? CreateImageSource()
        {
            if (!_initialized)
            {
                _initialized = true;
                _ = DriveIconUtility.CreateDriveIconAsync(_path,
                    image =>
                    {
                        _bitmapSource = image.GetBitmapSource(256.0);
                        DriveIconUtility.SetDriveIconCache(_path, _bitmapSource);
                        Changed?.Invoke(this, EventArgs.Empty);
                        RaisePropertyChanged("");
                    });
            }

            return DriveIconUtility.GetDriveIconCache(_path);
        }
    }
}
