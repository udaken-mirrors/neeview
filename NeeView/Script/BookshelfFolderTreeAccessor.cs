using System;

namespace NeeView
{
    public class BookshelfFolderTreeAccessor
    {
        private readonly BookshelfFolderTreeModel _model;

        public BookshelfFolderTreeAccessor(BookshelfFolderTreeModel model)
        {
            _model = model;

            QuickAccessNode = new RootQuickAccessNodeAccessor(_model, _model.RootQuickAccess ?? throw new InvalidOperationException());
            DirectoryNode = new DirectoryNodeAccessor(_model, _model.RootDirectory ?? throw new InvalidOperationException());
            BookmarkNode = new BookmarkNodeAccessor(_model, _model.RootBookmarkFolder ?? throw new InvalidOperationException());
        }


        [WordNodeMember(IsAutoCollect = false)]
        public RootQuickAccessNodeAccessor QuickAccessNode { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public DirectoryNodeAccessor DirectoryNode { get; }

        [WordNodeMember(IsAutoCollect = false)]
        public BookmarkNodeAccessor BookmarkNode { get; }

        [WordNodeMember]
        public NodeAccessor? SelectedItem
        {
            get { return _model.SelectedItem is not null ? FolderNodeAccessorFactory.Create(_model, _model.SelectedItem) : null; }
            set { AppDispatcher.Invoke(() => _model.SetSelectedItem(value?.Source)); }
        }


        [WordNodeMember]
        public void Expand(string path)
        {
            AppDispatcher.Invoke(() => _model.SyncDirectory(path));
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            node.Children?.Add(QuickAccessNode.CreateWordNode(nameof(QuickAccessNode)));
            node.Children?.Add(DirectoryNode.CreateWordNode(nameof(DirectoryNode)));
            node.Children?.Add(BookmarkNode.CreateWordNode(nameof(BookmarkNode)));
            return node;
        }
    }
}
