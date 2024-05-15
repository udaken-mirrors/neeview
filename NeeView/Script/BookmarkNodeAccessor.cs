using System.Linq;

namespace NeeView
{
    public record class BookmarkNodeAccessor : NodeAccessor
    {
        private readonly BookmarkFolderNode _node;

        public BookmarkNodeAccessor(FolderTreeModel model, BookmarkFolderNode name) : base(model, name)
        {
            _node = name;
        }

        [WordNodeMember]
        public BookmarkNodeAccessor[] Children
        {
            get { return GetChildren().OfType<BookmarkNodeAccessor>().ToArray(); }
        }

        [WordNodeMember]
        public BookmarkNodeAccessor? Parent
        {
            get { return GetParent() as BookmarkNodeAccessor; }
        }

        [WordNodeMember]
        public string Path
        {
            get { return _node.Path; }
        }

        [WordNodeMember]
        public virtual string Name
        {
            get { return _node.DispName; }
            set { AppDispatcher.Invoke(() => _node.Rename(value)); }
        }

        [WordNodeMember]
        public BookmarkNodeAccessor? Insert(int index, string name)
        {
            return AppDispatcher.Invoke(() =>
            {
                var item = Model.NewBookmarkFolder(_node);
                if (item is null) return null;
                item.Rename(name);
                return new BookmarkNodeAccessor(Model, item);
            });
        }

        [WordNodeMember]
        public void Remove(QuickAccessNodeAccessor item)
        {
            AppDispatcher.Invoke(() =>
            {
                Model.RemoveBookmarkFolder(_node);
            });
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }
}
