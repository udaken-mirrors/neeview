using NeeLaboratory.ComponentModel;
using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class QuickAccessCollection : BindableBase
    {
        static QuickAccessCollection() => Current = new QuickAccessCollection();
        public static QuickAccessCollection Current { get; }


        public QuickAccessCollection()
        {
            QuickAccessNode.NameChanged += QuickAccessNode_NameChanged;
        }


        public event EventHandler<QuickAccessCollectionChangeEventArgs>? CollectionChanged;

        private ObservableCollection<QuickAccess> _items = new();
        public ObservableCollection<QuickAccess> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }


        private void QuickAccessNode_NameChanged(object? sender, EventArgs e)
        {
            if (sender is not QuickAccessNode node) return;

            if (_items.Contains(node.QuickAccessSource))
            {
                CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Rename, node.QuickAccessSource));
            }
        }

        public void Insert(int index, QuickAccess item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            Items.Insert(index, item);
            CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Add, item));
        }

        public bool Remove(QuickAccess item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            var isRemoved = Items.Remove(item);
            CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Remove, item));
            return isRemoved;
        }

        public void Move(int srcIndex, int dstIndex)
        {
            if (srcIndex == dstIndex) return;

            var item = Items[srcIndex];

            Items.RemoveAt(srcIndex);
            CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Remove, item));

            Items.Insert(dstIndex, item);
            CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Add, item));
        }


        #region Memento
        [Memento]
        public class Memento
        {
            public List<QuickAccess>? Items { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Items = new List<QuickAccess>(this.Items);

            return memento;
        }

        public void Restore(Memento? memento)
        {
            if (memento == null) return;
            if (memento.Items is null) return;

            this.Items = new ObservableCollection<QuickAccess>(memento.Items);
            CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Refresh, null));
        }

        #endregion

    }


}
