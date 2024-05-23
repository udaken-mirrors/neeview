using System.Collections.Generic;

namespace NeeView
{
    public class DestinationFolderCollection : List<DestinationFolder>
    {
        public DestinationFolderCollection()
        {
        }

        public DestinationFolderCollection(IEnumerable<DestinationFolder> collection) : base(collection)
        {
        }

        public DestinationFolder CreateNew()
        {
            var item = new DestinationFolder();
            this.Add(item);
            return item;
        }
    }
}
