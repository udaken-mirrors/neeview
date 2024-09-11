using System;
using System.Collections;
using System.Collections.Generic;
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


        internal FolderTreeModel Model => _model;

        internal FolderTreeNodeBase Node => _node;

        [WordNodeMember]
        public bool IsDisposed
        {
            get { return _node.IsDisposed; }
        }

        [WordNodeMember]
        public virtual bool IsExpanded
        {
            get { return _node.IsExpanded; }
            set { AppDispatcher.Invoke(() => _node.IsExpanded = value); }
        }

        [WordNodeMember]
        public virtual string? Name => GetName();

        [WordNodeMember]
        public virtual NodeAccessor? Parent => GetParent();

        [WordNodeMember]
        public virtual NodeAccessor[]? Children => GetChildren();

        [WordNodeMember]
        public virtual object? Value => null;

        [WordNodeMember]
        public string? ValueType => Value?.GetType().Name;

        [WordNodeMember(IsEnabled = false)]
        public virtual NodeAccessor Add()
        {
            return Add(null);
        }

        [WordNodeMember(IsBaseClassOnly = true)]
        public virtual NodeAccessor Add(IDictionary<string, object?>? parameter)
        {
            if (Children is null) throw new NotSupportedException();

            return AppDispatcher.Invoke(() =>
            {
                var item = _model.NewNode(_node) ?? throw new InvalidOperationException("Cannot create new node.");
                var accessor = FolderNodeAccessorFactory.Create(_model, item);
                (accessor.Value as ISetParameter)?.SetParameter(parameter);
                return accessor;
            });
        }

        [WordNodeMember(IsEnabled = false)]
        public virtual NodeAccessor Insert(int index)
        {
            return Insert(index, null);
        }

        [WordNodeMember(IsBaseClassOnly = true, AltName = nameof(Add))]
        public virtual NodeAccessor Insert(int index, IDictionary<string, object?>? parameter)
        {
            if (Children is null) throw new NotSupportedException();

            return AppDispatcher.Invoke(() =>
            {
                var item = _model.NewNode(_node, index) ?? throw new InvalidOperationException("Cannot create new node.");
                var accessor = FolderNodeAccessorFactory.Create(_model, item);
                (accessor.Value as ISetParameter)?.SetParameter(parameter);
                return accessor;
            });
        }

        [WordNodeMember(IsBaseClassOnly = true)]
        public virtual bool Remove(NodeAccessor item)
        {
            if (Children is null) throw new NotSupportedException();

            if (this.Node != item.Node.Parent) return false;

            return AppDispatcher.Invoke(() => _model.RemoveNode(item.Node));
        }

        [WordNodeMember(IsBaseClassOnly = true, AltName =nameof(Remove))]
        public virtual bool RemoveAt(int index)
        {
            if (Children is null) throw new NotSupportedException();

            return AppDispatcher.Invoke(() => _model.RemoveNodeAt(this.Node, index));
        }

        [WordNodeMember(IsBaseClassOnly = true)]
        public virtual int IndexOf(NodeAccessor item)
        {
            if (Children is null) throw new NotSupportedException();

            if (this.Node != item.Node.Parent) return -1;

            return AppDispatcher.Invoke(() => this.Node.Children?.IndexOf(item.Node) ?? -1);
        }

        [WordNodeMember(IsBaseClassOnly = true)]
        public virtual void Move(int oldIndex, int newIndex)
        {
            if (Children is null) throw new NotSupportedException();

            AppDispatcher.Invoke(() => _model.MoveNode(this.Node, oldIndex, newIndex));
        }


        protected virtual string GetName()
        {
            return "";
        }

        protected NodeAccessor? GetParent()
        {
            var parent = _node.Parent;
            if (parent is null) return null;
            if (parent is RootFolderTree) return null;
            return FolderNodeAccessorFactory.Create(_model, parent);
        }

        protected NodeAccessor[]? GetChildren()
        {
            return _node.Children?.Select(e => FolderNodeAccessorFactory.Create(_model, e)).ToArray();
        }
    }
}
