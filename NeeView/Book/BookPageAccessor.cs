using NeeLaboratory;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ページ範囲に関する処理
    /// </summary>
    public class BookPageAccessor
    {
        public BookPageAccessor(IReadOnlyList<Page> pages)
        {
            Pages = pages;
        }

        public IReadOnlyList<Page> Pages { get; }

        public Page? First => Pages.FirstOrDefault();
        public Page? Last => Pages.LastOrDefault();

        public int FirstIndex => Pages.Any() ? 0 : -1;
        public int LastIndex => Pages.Any() ? Pages.Count - 1 : -1;

        public PagePosition FirstPosition => Pages.Any() ? PagePosition.Zero : PagePosition.Empty;
        public PagePosition LastPosition => Pages.Any() ? new(Pages.Count - 1, 1) : PagePosition.Empty;


        public bool ContainsIndex(int index)
        {
            return 0 <= index && index < Pages.Count;
        }

        public int ClampIndex(int index)
        {
            return MathUtility.Clamp(index, FirstIndex, LastIndex);
        }

        public Page? GetPage(int index)
        {
            if (ContainsIndex(index))
            {
                return Pages[index];
            }
            else
            {
                return null;
            }
        }

        public PagePosition ValidatePosition(PagePosition position)
        {
            if (position.IsEmpty() || !ContainsIndex(position.Index))
            {
                return PagePosition.Empty;
            }
            return position;
        }

        public PagePosition ClampPosition(PagePosition position)
        {
            return MathUtility.Clamp(position, FirstPosition, LastPosition);
        }
    }

}
