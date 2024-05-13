using System.Globalization;
using System.Linq;

namespace NeeView
{
    public record class DirectoryNodeAccessorBase : FolderNodeAccessor
    {
        private readonly DirectoryNode _node;

        public DirectoryNodeAccessorBase(FolderTreeModel model, DirectoryNode node) : base(model, node)
        {
            _node = node;
        }

        [WordNodeMember]
        public DirectoryNodeAccessor[] Children
        {
            get { return GetChildren().OfType<DirectoryNodeAccessor>().ToArray(); }
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
    }

    public record class RootDirectoryNodeAccessor : DirectoryNodeAccessorBase
    {
        public RootDirectoryNodeAccessor(FolderTreeModel model, RootDirectoryNode node) : base(model, node)
        {
        }

        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }

    public record class DirectoryNodeAccessor : DirectoryNodeAccessorBase
    {
        public DirectoryNodeAccessor(FolderTreeModel model, DirectoryNode node) : base(model, node)
        {
        }
    }
}
