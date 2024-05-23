using System.Linq;

namespace NeeView
{
    public class DestinationFolderCollectionAccessor
    {
        private readonly DestinationFolderCollection _collection;

        public DestinationFolderCollectionAccessor()
        {
            _collection = Config.Current.System.DestinationFolderCollection;
        }

        [WordNodeMember]
        public DestinationFolderAccessor[] Items
        {
            get { return _collection.Select(e => new DestinationFolderAccessor(e)).ToArray(); }
        }


        [WordNodeMember]
        public DestinationFolderAccessor CreateNew()
        {
            return AppDispatcher.Invoke(() => { return new DestinationFolderAccessor(_collection.CreateNew()); });
        }

        [WordNodeMember]
        public void Remove(DestinationFolderAccessor item)
        {
            AppDispatcher.Invoke(() => _collection.Remove(item.Source));
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            return node;
        }
    }
}
