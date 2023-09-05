using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public static class BookFactory
    {
        public static async Task<Book> CreateAsync(object? sender, BookAddress address, BookCreateSetting setting, BookMemento memento, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var bookSource = await BookSourceFactory.CreateAsync(address, setting, token);

#warning Media用処理未実装。 これはページ終端挙動の動画再生開始井chいの指定だが、どする？
#if false
            if (bookSource.IsMedia)
            {
                foreach (var mediaContent in bookSource.Pages.Select(e => e.ContentAccessor).OfType<MediaContent>())
                {
                    mediaContent.IsLastStart = setting.StartPage.StartPageType == BookStartPageType.LastPage;
                }
            }
#endif

            var book = new Book(address, bookSource, memento, setting.LoadOption, setting.IsNew);

            // HACK: Start() で行いたい
            book.SetStartPage(sender, setting.StartPage);

            return book;
        }
    }

}
