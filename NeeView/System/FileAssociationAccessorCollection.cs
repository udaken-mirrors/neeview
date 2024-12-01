using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class FileAssociationAccessorCollection : List<FileAssociationAccessor>
    {
        private readonly FileAssociationCollection _source;

        public FileAssociationAccessorCollection(FileAssociationCollection source)
        {
            _source = source;
            this.AddRange(source.Select(e => new FileAssociationAccessor(e)));
        }

        public void Flush()
        {
            bool isChanged = false;
            foreach (var item in this)
            {
                isChanged |= item.Flush();
            }
            if (isChanged)
            {
                _source.RefreshShellIcons();
            }
        }
    }

}