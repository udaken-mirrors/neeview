using System.Linq;

namespace NeeView
{
    public record class DirectoryNodeAccessor : NodeAccessor
    {
        private readonly DirectoryNode _node;

        public DirectoryNodeAccessor(FolderTreeModel model, DirectoryNode node) : base(model, node)
        {
            _node = node;
        }

        [WordNodeMember]
        public DirectoryNodeAccessor[] Children
        {
            get { return GetChildren().OfType<DirectoryNodeAccessor>().ToArray(); }
        }

        [WordNodeMember]
        public DirectoryNodeAccessor? Parent
        {
            get { return GetParent() as DirectoryNodeAccessor; }
        }

        [WordNodeMember]
        public string Path
        {
            get { return _node.Path; }
        }

        [WordNodeMember]
        public string Name
        {
            get { return _node.DispName; }
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }
}
