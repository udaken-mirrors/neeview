using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using NeeView.Maths;

namespace NeeView.PageFrames
{
    /// <summary>
    /// [Immutable]
    /// 表示ページ構成
    /// </summary>
    public record class PageFrameElement : IEquatable<PageFrameElement?>
    {
        private BookContext _context;

        public PageFrameElement(BookContext context, Page page, PageRange range, int direction, PageTerminal terminal)
        {
            Debug.Assert(range.IsOnePage());
            Debug.Assert(!range.IsEmpty());
            Debug.Assert(range.Min.Index == page.Index);
            Debug.Assert(range.Max.Index == page.Index);

            _context = context;

            PageRange = range;
            Terminal = terminal;
            Page = page;
            Direction = direction;

            RawSize = ViewSizeCalculator.GetViewSize();
        }

        // ページ
        public Page Page { get; }

        /// <summary>
        /// ページ範囲
        /// </summary>
        public PageRange PageRange { get; }

        /// <summary>
        /// ページ部位
        /// </summary>
        public PagePart PagePart => PagePartTools.CreatePagePart(PageRange, Direction);

        /// <summary>
        /// ブックの終端フラグ。ここ？
        /// </summary>
        public PageTerminal Terminal { get; }

        /// <summary>
        /// 表示スケール。２ページ表示で大きさを揃えるためのもの
        /// </summary>
        public double Scale { get; init; } = 1.0;

        /// <summary>
        /// 基準サイズ
        /// </summary>
        public Size RawSize { get; }

        /// <summary>
        /// スケールを適用した表示サイズ(幅)
        /// </summary>
        public double Width => RawSize.Width * Scale;

        /// <summary>
        /// スケールを適用した表示サイズ(高さ)
        /// </summary>
        public double Height => RawSize.Height * Scale;

        /// <summary>
        /// スケールを適用した表示サイズ
        /// </summary>
        public Size Size => new Size(Width, Height);

        /// <summary>
        /// ブック方向。これによって分割ページの場合に左右どちらのパーツであるかが決定する
        /// </summary>
        public int Direction { get; }

        /// <summary>
        /// ダミーページ
        /// </summary>
        public bool IsDummy { get; init; }
        
        public PageViewSizeCalculator ViewSizeCalculator => new PageViewSizeCalculator(_context, Page, PageRange, Direction);


        public bool Contains(Page page)
        {
            return PageRange.Min.Index <= page.Index && page.Index <= PageRange.Max.Index;
        }

        public bool Contains(PagePosition index)
        {
            return PageRange.IsContains(index);
        }

        public bool IsPageLandscape()
        {
            // TODO: _page.Size は Load によって変更される可能性があるのでこれは厳密には不正な処理になることがあるはず
            // TODO: ここで Config.Default を参照してるのはよろしくない
            return AspectRatioTools.IsLandscape(new PageSizeCalculator(_context, Page).GetPageSize(), Config.Current.Book.WideRatio);
        }

        public bool IsLandscape()
        {
            // TODO: ここで Config.Default を参照してるのはよろしくない
            return AspectRatioTools.IsLandscape(RawSize, Config.Current.Book.WideRatio);
        }

        public override string ToString()
        {
            return PageRange.ToString();
        }

        public bool IsMatch(PageFrameElement? other)
        {
            return other is not null
                && Page == other.Page
                && PageRange.Equals(other.PageRange)
                && Direction == other.Direction;
        }


#if false
        public override bool Equals(object? obj)
        {
            return Equals(obj as PageSource);
        }

        public bool Equals(PageSource? other)
        {
            return other is not null
                   && _range.Equals(other._range)
                   && _isLoaded == other._isLoaded
                   && _width == other._width
                   && _height == other._height
                   && _direction == other._direction;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_range, _isLoaded, _width, _height, _direction);
        }

        public static bool operator ==(PageSource? left, PageSource? right)
        {
            return EqualityComparer<PageSource>.Default.Equals(left, right);
        }

        public static bool operator !=(PageSource? left, PageSource? right)
        {
            return !(left == right);
        }
#endif
    }
}
