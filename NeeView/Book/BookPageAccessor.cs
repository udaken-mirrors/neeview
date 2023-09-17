using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class BookPageAccessor
    {
        private readonly IBookPageContext _book;

        public BookPageAccessor(IBookPageContext book)
        {
            _book = book;
        }

        public IReadOnlyList<Page> Pages => _book.Pages;

        public PagePosition FirstPosition => Pages.Any() ? PagePosition.Zero : PagePosition.Empty;
        public PagePosition LastPosition => Pages.Any() ? new(Pages.Count - 1, 1) : PagePosition.Empty;


        public bool ContainsIndex(int index)
        {
            return 0 <= index && index < Pages.Count;
        }

        public PagePosition ValidatePosition(PagePosition position)
        {
            if (position.IsEmpty() || !ContainsIndex(position.Index))
            {
                return PagePosition.Empty;
            }
            return position;
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

    }

}
