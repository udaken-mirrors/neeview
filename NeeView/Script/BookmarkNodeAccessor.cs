using System.Linq;

namespace NeeView
{
    public record class BookmarkFolderNodeAccessorBase : FolderNodeAccessor
    {
        private readonly BookmarkFolderNode _node;

        public BookmarkFolderNodeAccessorBase(FolderTreeModel model, BookmarkFolderNode name) : base(model, name)
        {
            _node = name;
        }


        protected BookmarkFolderNode Node => _node;


        [WordNodeMember]
        public string Path
        {
            get { return _node.Path; }
        }

        [WordNodeMember]
        public virtual string Name
        {
            get { return _node.DispName; }
            set { }
        }


        [WordNodeMember]
        public BookmarkNodeAccessor[] Children
        {
            get { return GetChildren().OfType<BookmarkNodeAccessor>().ToArray(); }
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
    }


    public record class RootBookmarkNodeAccessor : BookmarkFolderNodeAccessorBase
    {
        public RootBookmarkNodeAccessor(FolderTreeModel model, RootBookmarkFolderNode node) : base(model, node)
        {
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }

    public record class BookmarkNodeAccessor : BookmarkFolderNodeAccessorBase
    {
        public BookmarkNodeAccessor(FolderTreeModel model, BookmarkFolderNode node) : base(model, node)
        {
        }

        [WordNodeMember]
        public override string Name
        {
            get { return Node.Name; }
            set { AppDispatcher.Invoke(() => Node.Rename(value)); }
        }
    }
}
