namespace NeeView
{
    public class BookshelfFolderListPresenter : FolderListPresenter
    {
        private readonly FolderListView _folderListView;
        private readonly BookshelfFolderList _folderList;


        public BookshelfFolderListPresenter(FolderListView folderListView, BookshelfFolderList folderList) : base(folderListView, folderList)
        {
            _folderListView = folderListView;
            _folderList = folderList;

            folderListView.SearchBoxFocusChanged += FolderListView_SearchBoxFocusChanged;
        }


        public FolderListView FolderListView => _folderListView;

        public BookshelfFolderList FolderList => _folderList;


        private void FolderListView_SearchBoxFocusChanged(object? sender, FocusChangedEventArgs e)
        {
            if (FolderListBox is null) return;

            // リストのフォーカス更新を停止
            FolderListBox.IsFocusEnabled = !e.IsFocused;
        }

    }
}
