using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using NeeLaboratory.Linq;
using NeeView.ComponentModel;
using NeeView.Maths;

namespace NeeView.PageFrames
{
    /// <summary>
    /// PageFrame生成
    /// </summary>
    public class PageFrameFactory
    {
        private readonly BookPageAccessor _book;
        private readonly ContentSizeCalculator _calculator;
        private readonly BookContext _bookContext;
        private readonly PageFrameContext _context;


        public PageFrameFactory(PageFrameContext context, BookContext bookContext, ContentSizeCalculator calculator)
        {
            _bookContext = bookContext;
            _context = context;
            _book = new BookPageAccessor(_bookContext.Pages);
            _calculator = calculator;
        }


        public PageRange GetFirstTerminalRange()
        {
            return new PageRange(new PagePosition(-1, 0), 2);
        }

        public PageRange GetLastTerminalRange()
        {
            return new PageRange(new PagePosition(_book.Pages.Count, 0), 2);
        }


        /// <summary>
        /// フレーム作成
        /// </summary>
        /// <param name="position">開始座標</param>
        /// <param name="direction">作成方向</param>
        /// <returns>フレーム。範囲は正規化されている</returns>
        public PageFrame? CreatePageFrame(PagePosition position, int direction)
        {
            if (!_context.IsLoopPage && !_book.ContainsIndex(position.Index))
            {
                return null;
            }

            Debug.Assert(IsValidFramePosition(position, direction));

            var frame = CreatePageFrame(CreatePageSource(position, direction), direction);
            Debug.Assert(frame != null);
            Debug.Assert(frame.FrameRange.Contains(position));
            frame.AssertValid(_context);

            return frame;
        }

        /// <summary>
        /// フレーム開始座標修正。分割されない設定での座標修正など。
        /// </summary>
        /// <param name="position">修正座標</param>
        /// <param name="direction">フレーム方向</param>
        /// <returns>修正された座標</returns>
        private PagePosition FixFramePosition(PagePosition position, int direction)
        {
            var page = _book.GetPage(position.Index);
            if (page is null) return PagePosition.Empty;

            // 分割可能であれば修正不要
            if (_context.PageMode == PageMode.SinglePage && _context.IsSupportedDividePage && IsLandscape(page))
            {
                return position;
            }

            // 分割不可なのでフレーム方向に応じたページ先頭座標に修正
            return new PagePosition(position.Index, direction < 0 ? 1 : 0);
        }

        /// <summary>
        /// 固定サイズやトリミングを反映させたサイズで横長判定
        /// </summary>
        /// <param name="page">ページ</param>
        /// <returns>横長？</returns>
        private bool IsLandscape(Page page)
        {
            return AspectRatioTools.IsLandscape(new PageSizeCalculator(_context, page).GetPageSize());
        }

        /// <summary>
        /// フレーム開始座標として適切か判定
        /// </summary>
        /// <param name="position">開始座標</param>
        /// <param name="direction">フレーム方向</param>
        /// <returns>適切であれば true</returns>
        private bool IsValidFramePosition(PagePosition position, int direction)
        {
            if (_context.IsLoopPage)
            {
                position = _book.NormalizePosition(position);
            }

            return position == FixFramePosition(position, direction);
        }

        /// <summary>
        /// PageSource作成
        /// </summary>
        /// <param name="position">基準座標</param>
        /// <param name="direction">作成方向</param>
        /// <returns></returns>
        private PageFrameElement? CreatePageSource(PagePosition position, int direction)
        {
            if (position.IsEmpty())
            {
                return null;
            }

            var page = _book.GetPage(position.Index, _context.IsLoopPage);
            if (page is null)
            {
                return null;
            }

            var range = PageRange.CreatePageRangeForOnePage(position, direction);

            // 分割ページ
            if (_context.PageMode == PageMode.SinglePage && _context.IsSupportedDividePage && IsLandscape(page))
            {
                range = new PageRange(range.Top(direction), direction);
            }

            return new PageFrameElement(_context, _bookContext, page, range, _context.ReadOrder.ToSign(), GetTerminal(range));
        }

        private PageTerminal GetTerminal(PageRange range)
        {
            var first = range.Min == _book.FirstPosition ? PageTerminal.First : PageTerminal.None;
            var last = range.Max == _book.LastPosition ? PageTerminal.Last : PageTerminal.None;
            return first | last;
        }


        private PageFrame? CreatePageFrame(PageFrameElement? source, int direction)
        {
            var source1 = source;
            if (source1 is null) return null;

            if (_context.FramePageSize == 2 && !_bookContext.IsMedia)
            {
                // TODO: SinglePageFrame 作成が分散しているのでまとめる？
                if (_context.IsSupportedWidePage && source1.IsLandscape())
                {
                    return CreateSinglePageFrame(source1);
                }

                var position = _book.ValidatePosition(source1.PageRange.Next(direction), _context.IsLoopPage);
                var source2 = CreatePageSource(position, direction);
                if (source2 is null)
                {
                    return CreateWideFillPageFrame(source1);
                }

                if (_context.IsSupportedWidePage && source2.IsLandscape())
                {
                    return CreateWideFillPageFrame(source1);
                }

                bool isSingleFirstPage = _context.IsSupportedSingleFirstPage && (direction < 0 ? source2 : source1).PageRange.Min.Index == _book.FirstPosition.Index;
                bool isSingleLastPage = _context.IsSupportedSingleLastPage && (direction < 0 ? source1 : source2).PageRange.Min.Index == _book.LastPosition.Index;
                if (isSingleFirstPage || isSingleLastPage)
                {
                    return CreateWideFillPageFrame(source1);
                }

                return CreateWidePageFrame(source1, source2, direction);
            }
            else
            {
                return CreateSinglePageFrame(source1);
            }
        }

        /// <summary>
        /// 1エレメントのシングルフレームを作成
        /// </summary>
        /// <param name="source1"></param>
        /// <returns></returns>
        private PageFrame CreateSinglePageFrame(PageFrameElement source1)
        {
            var bookDirection = _context.ReadOrder.ToSign();
            return new PageFrame(source1, bookDirection, _calculator);
        }

        /// <summary>
        /// 1エレメントのワイドフレームを作成。
        /// 可能であればダミーページを追加する。
        /// </summary>
        /// <param name="source1"></param>
        /// <returns></returns>
        private PageFrame CreateWideFillPageFrame(PageFrameElement source1)
        {
            if (_context.IsInsertDummyPage && _book.Pages.Count > 1)
            {
                var source2 = source1 with { IsDummy = true };
                return CreateWidePageFrame(source1, source2, 1);
            }
            else
            {
                return CreateSinglePageFrame(source1);
            }
        }

        /// <summary>
        /// 2エレメントのワイドフレームを作成
        /// </summary>
        /// <param name="source1"></param>
        /// <param name="source2"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private PageFrame CreateWidePageFrame(PageFrameElement source1, PageFrameElement source2, int direction)
        {
            var sources = new List<PageFrameElement>() { source1, source2 }.Direction(direction);

            var bookDirection = _context.ReadOrder.ToSign();

            // content size alignment
            var scales = _calculator.CalcContentScale(sources.Select(e => e.RawSize));
            var scaledSources = sources
                .Select((source, index) => (source, index))
                .Select(e => scales[e.index] == e.source.Scale ? e.source : e.source with { Scale = scales[e.index] });

            return new PageFrame(scaledSources, bookDirection, _calculator);
        }
    }

}
