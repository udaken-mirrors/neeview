﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// サムネイル画像 固定リソース
    /// </summary>
    public static class ThumbnailResource
    {
        public static readonly byte[] EmptyImage = System.Text.Encoding.ASCII.GetBytes("EMPTY!");
        public static readonly byte[] MediaImage = System.Text.Encoding.ASCII.GetBytes("MEDIA!");
        public static readonly byte[] FolderImage = System.Text.Encoding.ASCII.GetBytes("FOLDER!");
        public static readonly SolidColorBrush MediaBackground = new(Color.FromRgb(0x3A, 0x3A, 0x3A));

        private static ImageSource? _emptyImageSource;
        private static BitmapSource? _emptyBitmapSource;
        private static BitmapSource? _mediaBitmapSource;
        private static BitmapSource? _folderBitmapSource;


        /// <summary>
        /// イメージ初期化
        /// UIスレッドで実行すること。
        /// </summary>
        public static void InitializeStaticImages()
        {
            Debug.Assert(Thread.CurrentThread.GetApartmentState() == ApartmentState.STA);

            _ = EmptyImageSource;
            _ = EmptyBitmapSource;
            _ = MediaBitmapSource;
            _ = FolderBitmapSource;
        }

        public static ImageSource EmptyImageSource
        {
            get
            {
                if (_emptyImageSource == null)
                {
                    _emptyImageSource = MainWindow.Current.Resources["thumbnail_default"] as ImageSource
                        ?? throw new InvalidOperationException("Cannot found resource");
                }
                return _emptyImageSource;
            }
        }

        public static BitmapSource EmptyBitmapSource
        {
            get
            {
                if (_emptyBitmapSource == null)
                {
                    _emptyBitmapSource = CreatetResourceBitmapImage("/Resources/Empty.png");
                }
                return _emptyBitmapSource;
            }
        }

        public static BitmapSource MediaBitmapSource
        {
            get
            {
                if (_mediaBitmapSource == null)
                {
                    _mediaBitmapSource = CreatetResourceBitmapImage("/Resources/Media.png");
                }
                return _mediaBitmapSource;
            }
        }

        public static BitmapSource FolderBitmapSource
        {
            get
            {
                if (_folderBitmapSource == null)
                {
                    _folderBitmapSource = FileIconCollection.Current.CreateDefaultFolderIcon().GetBitmapSource(256.0) ?? EmptyBitmapSource;
                }
                return _folderBitmapSource;
            }
        }

        private static BitmapImage CreatetResourceBitmapImage(string path)
        {
            var uri = new Uri("pack://application:,,," + path);
            var bitmap = new BitmapImage(uri);
            bitmap.Freeze();

            return bitmap;
        }
    }
}