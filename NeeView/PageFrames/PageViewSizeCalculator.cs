using System;
using System.Windows;

namespace NeeView.PageFrames
{
    public class PageSizeCalculator
    {
        private readonly PageDataSource _pageDataSource;
        private readonly PageCustomSize _customSize;
        private readonly ImageTrimConfig _imageTrim;


        public PageSizeCalculator(PageFrameContext context, PageDataSource pageDataSource)
        {
            _pageDataSource = pageDataSource;
            _imageTrim = context.ImageTrimConfig;
            _customSize = new PageCustomSize(context.ImageCustomSizeConfig, context);
        }


        /// <summary>
        /// ページサイズ
        /// </summary>
        /// <returns>画像サイズから固定サイズとトリミングを反映したサイズ</returns>
        public Size GetPageSize()
        {
            var size = _customSize.TransformToCustomSize(_pageDataSource.Size);

            if (_imageTrim.IsEnabled)
            {
                var width = Math.Max(size.Width - size.Width * (_imageTrim.Left + _imageTrim.Right), 0.0);
                var height = Math.Max(size.Height - size.Height * (_imageTrim.Top + _imageTrim.Bottom), 0.0);
                size = new Size(width, height);
            }

            return size;
        }
    }



    public class PageViewSizeCalculator
    {
        private readonly PageRange _pagePart;
        private readonly int _direction;
        private readonly ImageTrimConfig _imageTrim;
        private readonly PageSizeCalculator _pageSizeCalculator;

        public PageViewSizeCalculator(PageFrameContext context, PageDataSource pageDataSource, PageRange pagePart, int direction)
        {
            _pagePart = pagePart;
            _direction = direction;
            _imageTrim = context.ImageTrimConfig;

            _pageSizeCalculator = new PageSizeCalculator(context, pageDataSource);
        }



        /// <summary>
        /// 表示サイズ
        /// </summary>
        /// <returns>ページサイズに分割を適用したサイズ</returns>
        public Size GetViewSize()
        {
            var size = _pageSizeCalculator.GetPageSize();

            return _pagePart.PartSize switch
            {
                0 => new Size(0.0, size.Height),
                1 => new Size(Math.Floor(size.Width * 0.5 + 0.4), size.Height),
                _ => size,
            };
        }

        /// <summary>
        /// 実表示サイズから画像サイズ逆算
        /// </summary>
        /// <param name="viewSize">実表示サイズ</param>
        /// <returns>必要な画像サイズ</returns>
        public Size GetSourceSize(Size viewSize)
        {
            // TODO: 逆算するのなら最初から所持しておくべきでは？

            var width = viewSize.Width;
            var height = viewSize.Height;

            // ページ分割逆補正
            if (_pagePart.PartSize == 1)
            {
                width = width * 2.0;
            }

            // トリミング逆補正
            if (_imageTrim.IsEnabled)
            {
                var wRate = Math.Max(1.0 - (_imageTrim.Left + _imageTrim.Right), 0.0);
                var hRate = Math.Max(1.0 - (_imageTrim.Top + _imageTrim.Bottom), 0.0);

                if (wRate > 0.0 && hRate > 0.0)
                {
                    width = width / wRate;
                    height = height / hRate;
                }
            }

            return new Size(width, height);
        }

        public Rect GetViewBox()
        {
            var crop = new Rect(0.0, 0.0, 1.0, 1.0);

            // トリミング
            if (_imageTrim.IsEnabled)
            {
                var x = crop.X + _imageTrim.Left;
                var width = Math.Max(crop.Width - (_imageTrim.Left + _imageTrim.Right), 0.0);
                var y = crop.Y + _imageTrim.Top;
                var height = Math.Max(crop.Height - (_imageTrim.Top + _imageTrim.Bottom), 0.0);
                crop = new Rect(x, y, width, height);
            }

            // ページパートで領域分割
            crop = CropByPagePart(_pagePart, _direction, crop);

            // NOTE: ポリゴンの歪み補正
            crop.Offset(new Vector(-0.00001, -0.00001));

            return crop;
        }

        private static Rect CropByPagePart(PageRange pagePart, int direction, Rect rect)
        {
            switch (pagePart.PartSize)
            {
                case 0:
                    return new Rect(rect.X, rect.Y, 0.0, rect.Height);

                case 1:
                    bool isLeftPart = pagePart.Min.Part == 0;
                    if (direction == -1) isLeftPart = !isLeftPart;
                    double half = rect.Width * 0.5;
                    double left = isLeftPart ? rect.X : rect.X + half;
                    return new Rect(left, rect.Y, half, rect.Height);

                default:
                    return rect;
            }
        }
    }
}
