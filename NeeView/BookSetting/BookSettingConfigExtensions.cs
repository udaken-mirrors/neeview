using System.Diagnostics.CodeAnalysis;

namespace NeeView
{
    public static class BookSettingConfigExtensions
    {
        [return: NotNullIfNotNull("memento")]
        public static BookSettingConfig? FromBookMement(BookMemento? memento)
        {
            if (memento == null) return null;

            var collection = new BookSettingConfig();

            collection.Page = memento.Page ?? "";
            collection.PageMode = memento.PageMode;
            collection.BookReadOrder = memento.BookReadOrder;
            collection.IsSupportedDividePage = memento.IsSupportedDividePage;
            collection.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            collection.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            collection.IsSupportedWidePage = memento.IsSupportedWidePage;
            collection.IsRecursiveFolder = memento.IsRecursiveFolder;
            collection.SortMode = memento.SortMode;

            return collection;
        }

        public static BookMemento ToBookMemento(this BookSettingConfig self)
        {
            var memento = new BookMemento();

            memento.Page = self.Page;
            memento.PageMode = self.PageMode;
            memento.BookReadOrder = self.BookReadOrder;
            memento.IsSupportedDividePage = self.IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = self.IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = self.IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = self.IsSupportedWidePage;
            memento.IsRecursiveFolder = self.IsRecursiveFolder;
            memento.SortMode = self.SortMode;

            return memento;
        }

        public static void CopyTo(this BookSettingConfig self, BookSettingConfig target)
        {
            target.Page = self.Page;
            target.PageMode = self.PageMode;
            target.BookReadOrder = self.BookReadOrder;
            target.IsSupportedDividePage = self.IsSupportedDividePage;
            target.IsSupportedSingleFirstPage = self.IsSupportedSingleFirstPage;
            target.IsSupportedSingleLastPage = self.IsSupportedSingleLastPage;
            target.IsSupportedWidePage = self.IsSupportedWidePage;
            target.IsRecursiveFolder = self.IsRecursiveFolder;
            target.SortMode = self.SortMode;
        }
    }

}
