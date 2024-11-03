using System;

namespace NeeView
{
    public class BookshelfFolderTreeAccessor
    {
        private readonly FolderTreeModel _model;

        public BookshelfFolderTreeAccessor()
        {
            _model = AppDispatcher.Invoke(() => FolderPanel.Current.FolderTreeModel);
            QuickAccessNode = new QuickAccessFolderNodeAccessor(_model, _model.RootQuickAccess ?? throw new InvalidOperationException());
            DirectoryNode = new DirectoryNodeAccessor(_model, _model.RootDirectory ?? throw new InvalidOperationException());
            BookmarkNode = new BookmarkFolderNodeAccessor(_model, _model.RootBookmarkFolder ?? throw new InvalidOperationException());
        }


        [WordNodeMember(IsAutoCollect = false)]
        public QuickAccessFolderNodeAccessor QuickAccessNode { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public DirectoryNodeAccessor DirectoryNode { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookmarkFolderNodeAccessor BookmarkNode { get; }

        [WordNodeMember]
        public NodeAccessor? SelectedItem
        {
            get { return _model.SelectedItem is not null ? FolderNodeAccessorFactory.Create(_model, _model.SelectedItem) : null; }
            set { AppDispatcher.Invoke(() => _model.SetSelectedItem(value?.Node)); }
        }


        [WordNodeMember]
        public void Expand(string path)
        {
            AppDispatcher.Invoke(() => _model.SyncDirectory(path));
        }

        internal static WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, typeof(BookshelfFolderTreeAccessor));
            node.Children?.Add(QuickAccessFolderNodeAccessor.CreateWordNode(nameof(QuickAccessNode)));
            node.Children?.Add(DirectoryNodeAccessor.CreateWordNode(nameof(DirectoryNode)));
            node.Children?.Add(BookmarkFolderNodeAccessor.CreateWordNode(nameof(BookmarkNode)));
            return node;
        }
    }
}
