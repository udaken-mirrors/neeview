using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace NeeView
{
    public record class QuickAccessNodeAccessor : NodeAccessor
    {
        private readonly QuickAccessNode _node;
        private readonly QuickAccessNodeSource _value;

        public QuickAccessNodeAccessor(FolderTreeModel model, QuickAccessNode node) : base(model, node)
        {
            _node = node;
            _value = new QuickAccessNodeSource(_node);
        }


        [WordNodeMember(AltName = "@QuickAccessNodeSource")]
        [ReturnType(typeof(QuickAccessNodeSource))]
        public override object? Value => _value;


        [WordNodeMember(IsEnabled = false)]
        public override bool IsExpanded
        {
            get => false;
            set { }
        }

        [WordNodeMember(IsEnabled = false)]
        public override NodeAccessor[]? Children => base.Children;


        [WordNodeMember(AltClassType = typeof(NodeAccessor))]
        public override void MoveTo(int newIndex)
        {
            base.MoveTo(newIndex);
        }

        protected override string GetName() => _value.Name;
    }
}
