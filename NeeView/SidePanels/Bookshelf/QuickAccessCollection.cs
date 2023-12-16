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


        private ObservableCollection<QuickAccess> _items = new();


        public QuickAccessCollection()
        {
        }


        public event EventHandler<QuickAccessCollectionChangeEventArgs>? CollectionChanged;


        public ObservableCollection<QuickAccess> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    _items.ForEach(DetachItem);
                    _items = value;
                    _items.ForEach(AttachItem);
                    RaisePropertyChanged();
                }
            }
        }


        private void AttachItem(QuickAccess item)
        {
            item.PropertyChanged += QuickAccess_PropertyChanged;
        }

        private void DetachItem(QuickAccess item)
        {
            item.PropertyChanged -= QuickAccess_PropertyChanged;
        }

        private void QuickAccess_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not QuickAccess item) return;

            switch (e.PropertyName)
            {
                case nameof(QuickAccess.Name):
                    CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Rename, item));
                    break;
                case nameof(QuickAccess.Path):
                    CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.PathChanged, item));
                    break;
            }
        }

        public void Insert(int index, QuickAccess item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            Items.Insert(index, item);
            AttachItem(item);
            CollectionChanged?.Invoke(this, new QuickAccessCollectionChangeEventArgs(QuickAccessCollectionChangeAction.Add, item));
        }

        public bool Remove(QuickAccess item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            DetachItem(item);
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
