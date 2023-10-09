using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 固定サムネイル.
    /// 画像リソースを外部から指定する
    /// </summary>
    public class ConstThumbnail : IThumbnail
    {
        private ImageSource? _bitmapSource;
        protected Func<ImageSource?>? _create;

        public ConstThumbnail()
        {
        }

        public ConstThumbnail(ImageSource source)
        {
            _bitmapSource = source;
        }

        public ConstThumbnail(Func<ImageSource> crate)
        {
            _create = crate;
        }


        public event EventHandler? Changed;


        public bool IsValid => true;
        public bool IsUniqueImage => false;
        public bool IsNormalImage => false;
        public Brush Background => Brushes.Transparent;


        public ImageSource? CreateImageSource()
        {
            return _bitmapSource ?? _create?.Invoke();
        }
    }


    /// <summary>
    /// フォルダーサムネイル
    /// </summary>
    public class FolderThumbnail : ConstThumbnail
    {
        public FolderThumbnail()
        {
            _create = () => FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0);
        }
    }


    /// <summary>
    /// リソースサムネイル
    /// </summary>
    public class ResourceThumbnail : ConstThumbnail
    {
        public ResourceThumbnail(string resourceName, FrameworkElement? source = null)
        {
            var resources = source != null ? source.Resources : App.Current.Resources;
            _create = () => resources[resourceName] as ImageSource;
        }
    }
}
