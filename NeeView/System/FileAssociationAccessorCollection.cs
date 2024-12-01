using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class FileAssociationAccessorCollection : List<FileAssociationAccessor>
    {
        public FileAssociationAccessorCollection(FileAssociationCollection source)
        {
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
                FileAssociationTools.RefreshShellIcons();
            }
        }
    }

}