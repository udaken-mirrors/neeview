namespace NeeView
{
    public record class QuickAccessNodeAccessor : FolderNodeAccessor
    {
        private readonly QuickAccessNode _node;

        public QuickAccessNodeAccessor(FolderTreeModel model, QuickAccessNode node) : base(model, node)
        {
            _node = node;
        }

        [WordNodeMember]
        public string Path
        {
            get { return _node.Path; }
            set { AppDispatcher.Invoke(() => _node.SetPath(value)); }
        }

        [WordNodeMember]
        public string Name
        {
            get { return _node.Name; }
            set { AppDispatcher.Invoke(() => _node.Rename(value)); }
        }
    }
}
