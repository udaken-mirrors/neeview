using System.Linq;

namespace NeeView
{
    public record class RootQuickAccessNodeAccessor : FolderNodeAccessor
    {
        private readonly RootQuickAccessNode _node;
        private readonly QuickAccessCollection _collection;


        public RootQuickAccessNodeAccessor(FolderTreeModel model, RootQuickAccessNode node) : base(model, node)
        {
            _node = node;
            _collection = QuickAccessCollection.Current;
        }

        public string Name => _node.DispName;


        [WordNodeMember]
        public virtual QuickAccessNodeAccessor[] Children
        {
            get { return GetChildren().OfType<QuickAccessNodeAccessor>().ToArray(); }
        }

        [WordNodeMember]
        public QuickAccessNodeAccessor Insert(int index, string path)
        {
            return AppDispatcher.Invoke(() =>
            {
                var item = new QuickAccess(path);
                _collection.Insert(index, item);
                return new QuickAccessNodeAccessor(Model, _node.Children.Cast<QuickAccessNode>().First(e => e.QuickAccessSource == item));
            });
        }

        [WordNodeMember]
        public void Remove(QuickAccessNodeAccessor item)
        {
            AppDispatcher.Invoke(() =>
            {
                _collection.Remove(((QuickAccessNode)item.Source).QuickAccessSource);
            });
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }
}
