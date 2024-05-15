using System;
using System.Linq;

namespace NeeView
{
    [DocumentableBaseClass(typeof(NodeAccessor))]
    public record class NodeAccessor
    {
        private readonly FolderTreeModel _model;
        private readonly FolderTreeNodeBase _node;

        public NodeAccessor(FolderTreeModel model, FolderTreeNodeBase node)
        {
            _model = model;
            _node = node;
        }


        protected FolderTreeModel Model => _model;

        internal FolderTreeNodeBase Source => _node;


#if false
        [WordNodeMember]
        public string Type => this.GetType().Name;
#endif

        [WordNodeMember]
        public bool IsExpanded
        {
            get { return _node.IsExpanded; }
            set { AppDispatcher.Invoke(() => _node.IsExpanded = value); }
        }


        protected NodeAccessor? GetParent()
        {
            if (_node.Parent is null) return null;
            if (_node.Parent is RootFolderTree) return null;
            return FolderNodeAccessorFactory.Create(_model, _node.Parent);
        }

        protected NodeAccessor[] GetChildren()
        {
            return _node.Children?.Select(e => FolderNodeAccessorFactory.Create(_model, e)).ToArray() ?? Array.Empty<NodeAccessor>();
        }
    }
}
