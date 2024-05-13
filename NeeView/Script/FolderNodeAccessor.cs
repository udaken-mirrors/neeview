using System;
using System.Linq;

namespace NeeView
{
    public record class FolderNodeAccessor
    {
        private readonly FolderTreeModel _model;
        private readonly FolderTreeNodeBase _node;

        public FolderNodeAccessor(FolderTreeModel model, FolderTreeNodeBase node)
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

        [WordNodeMember]
        public FolderNodeAccessor? Parent
        {
            get
            {
                if (_node.Parent is null) return null;
                if (_node.Parent is RootFolderTree) return null;
                return FolderNodeAccessorFactory.Create(_model, _node.Parent);
            }
        }

        protected FolderNodeAccessor[] GetChildren()
        {
            return _node.Children?.Select(e => FolderNodeAccessorFactory.Create(_model, e)).ToArray() ?? Array.Empty<FolderNodeAccessor>();
        }
    }
}
