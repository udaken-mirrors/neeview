using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// 先読みページ範囲を求める
    /// </summary>
    public class BookAheadCalculator
    {
        private readonly BookSource _book;

        public BookAheadCalculator(BookSource book)
        {
            _book = book;
        }

        /// <summary>
        /// 先読みページ範囲を求める
        /// </summary>
        /// <returns>先読みページ範囲。ブック終端をふまえた2範囲を返す</returns>
        public List<PageRange> CreateAheadPageRange(PageRange source)
        {
            if (!AllowPreLoad() || Config.Current.Performance.PreLoadSize < 1)
            {
                return new List<PageRange>() { PageRange.Empty, PageRange.Empty };
            }

            PageRange range0 = CreateAheadPageRange(source, source.Direction, Config.Current.Performance.PreLoadSize);

            PageRange range1 = PageRange.Empty;
            if (range0.PageSize < Config.Current.Performance.PreLoadSize)
            {
                var size = Config.Current.Performance.PreLoadSize - range0.PageSize;
                range1 = CreateAheadPageRange(source, source.Direction * -1, size);
            }

            return new List<PageRange>() { range0, range1 };
        }

        // 先読み許可フラグ
        private static bool AllowPreLoad()
        {
            return Config.Current.Performance.PreLoadSize > 0;
        }

        private PageRange CreateAheadPageRange(PageRange source, int direction, int size)
        {
            int index = source.Next(direction).Index;
            var pos0 = new PagePosition(index, 0);
            var pos1 = new PagePosition(_book.Pages.ClampPageNumber(index + (size - 1) * direction), 0);
            var range = _book.Pages.IsValidPosition(pos0) ? new PageRange(pos0, pos1) : PageRange.Empty;

            return range;
        }



    }
}
