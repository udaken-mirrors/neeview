using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public static class BookFactory
    {
        public static async Task<Book> CreateAsync(object? sender, BookAddress address, BookCreateSetting setting, Book.Memento memento, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var bookSource = await BookSourceFactory.CreateAsync(address, setting, token);

            if (bookSource.IsMedia)
            {
                foreach (var mediaContent in bookSource.Pages.Select(e => e.ContentAccessor).OfType<MediaContent>())
                {
                    mediaContent.IsLastStart = setting.StartPage.StartPageType == BookStartPageType.LastPage;
                }
            }

            var book = new Book(address, bookSource, memento, setting.LoadOption, setting.IsNew);

            // HACK: Start() で行いたい
            book.SetStartPage(sender, setting.StartPage);

            return book;
        }
    }

}
