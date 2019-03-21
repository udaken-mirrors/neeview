﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NeeView
{
    /// <summary>
    /// MediaPlayer コンテンツ
    /// </summary>
    public class MediaContent : BitmapContent
    {
        public override bool IsLoaded => FileProxy != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MediaContent(ArchiveEntry entry) : base(entry)
        {
            IsAnimated = true;
            PictureInfo = new PictureInfo(entry);
        }

        public override PictureInfo PictureInfo { get; }

        /// <summary>
        /// サイズ設定
        /// </summary>
        public void SetSize(Size size)
        {
            this.Size = size;

            this.PictureInfo.Size = size;
            this.PictureInfo.OriginalSize = size;
        }

        /// <summary>
        /// コンテンツロード.
        /// </summary>
        public override async Task LoadAsync(CancellationToken token)
        {
            if (IsLoaded) return;

            ////this.PictureInfo.BitsPerPixel = 32;

            this.Size = new Size(704, 396);

            if (!token.IsCancellationRequested)
            {
                // TempFileに出力し、これをMediaPlayerに再生させる
                CreateTempFile(true);

                RaiseLoaded();
                RaiseChanged();
            }

            // サムネイル作成
            if (Thumbnail.IsValid) return;
            Thumbnail.Initialize(ThumbnailType.Media);

            await Task.CompletedTask;
        }
    }

}
