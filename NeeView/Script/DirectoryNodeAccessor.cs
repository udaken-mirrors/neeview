using System;
using System.Linq;

namespace NeeView
{
    public record class DirectoryNodeAccessor : NodeAccessor
    {
        private readonly DirectoryNode _node;
        private readonly DirectoryNodeSource _value;

        public DirectoryNodeAccessor(FolderTreeModel model, DirectoryNode node) : base(model, node)
        {
            _node = node;
            _value = new DirectoryNodeSource(_node);
        }


        [WordNodeMember(AltName = "@DirectoryNodeSource")]
        [ReturnType(typeof(DirectoryNodeSource))]
        public override object? Value => _value;


        [WordNodeMember(AltClassType = typeof(NodeAccessor))]
        public override NodeAccessor[]? Children
        {
            get
            {
                if (_node.ChildrenRaw is null)
                {
                    _node.CreateChildren(false);
                }
                return GetChildren() ?? [];
            }
        }


        protected override string GetName() => _value.Name;

        [WordNodeMember(AltClassType = typeof(NodeAccessor))]
        public override int IndexOf(NodeAccessor item)
        {
            return base.IndexOf(item);
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }
}
