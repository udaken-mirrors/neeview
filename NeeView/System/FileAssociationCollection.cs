using NeeView.Interop;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    public class FileAssociationCollection : List<FileAssociation>
    {
        public void Add(string extension, FileAssociationCategory category, string? description = null)
        {
            var association = new FileAssociation(extension, category) { Description = description };
            this.Add(association);
        }

        public bool TryAdd(string extension, FileAssociationCategory category, string? description = null)
        {
            if (this.Any(e => e.Extension == extension)) return false;
            Add(extension, category, description);
            return true;
        }
    }

}