using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace NeeView
{
    public record class QuickAccessFolderNodeAccessor : NodeAccessor
    {
        private readonly RootQuickAccessNode _node;
        private readonly QuickAccessCollection _collection;
        private readonly QuickAccessFolderNodeSource _value;

        // QuickAccessFolder。今のところ Root のみ。
        public QuickAccessFolderNodeAccessor(FolderTreeModel model, RootQuickAccessNode node) : base(model, node)
        {
            _node = node;
            _collection = QuickAccessCollection.Current;
            _value = new QuickAccessFolderNodeSource(_node);
        }


        [WordNodeMember(AltName = "@QuickAccessFolderNodeSource")]
        [ReturnType(typeof(QuickAccessFolderNodeSource))]
        public override object? Value => _value;


        protected override string GetName() => _value.Name;


        [WordNodeMember]
        [ReturnType(typeof(QuickAccessNodeAccessor))]
        public override NodeAccessor Add(IDictionary<string, object?>? parameter)
        {
            return base.Add(parameter);
        }

        [WordNodeMember(AltName = nameof(Add))]
        [ReturnType(typeof(QuickAccessNodeAccessor))]
        public override NodeAccessor Insert(int index, IDictionary<string, object?>? parameter)
        {
            return base.Insert(index, parameter);
        }

        [WordNodeMember(IsEnabled = false)]
        public override bool Remove()
        {
            throw new NotSupportedException();
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }

}
