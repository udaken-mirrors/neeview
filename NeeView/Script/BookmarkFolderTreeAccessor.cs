using System;

namespace NeeView
{
    public class BookmarkFolderTreeAccessor
    {
        private readonly FolderTreeModel _model;

        public BookmarkFolderTreeAccessor()
        {
            _model = AppDispatcher.Invoke(() => BookmarkPanel.Current.FolderTreeModel);
            BookmarkNode = new BookmarkFolderNodeAccessor(_model, _model.RootBookmarkFolder ?? throw new InvalidOperationException());
        }


        [WordNodeMember(IsAutoCollect = false)]
        public BookmarkFolderNodeAccessor BookmarkNode { get; }

        [WordNodeMember]
        public NodeAccessor? SelectedItem
        {
            get { return _model.SelectedItem is not null ? FolderNodeAccessorFactory.Create(_model, _model.SelectedItem) : null; }
            set { AppDispatcher.Invoke(() => _model.SetSelectedItem(value?.Node)); }
        }


        internal static WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, typeof(BookmarkFolderTreeAccessor));
            node.Children?.Add(BookmarkFolderNodeAccessor.CreateWordNode(nameof(BookmarkNode)));
            return node;
        }
    }

}
