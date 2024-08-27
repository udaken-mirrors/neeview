using NeeLaboratory.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace NeeView
{
    public class PlaylistItemCommandResource : PageCommandResource<PlaylistItem>
    {
        protected override Page? GetSelectedPage(object sender)
        {
            return ((sender as ListBox)?.SelectedItem as PlaylistItem)?.ArchivePage;
        }

        protected override List<Page>? GetSelectedPages(object sender)
        {
            return (sender as ListBox)?.SelectedItems?
                .Cast<PlaylistItem>()
                .WhereNotNull()
                .Select(e => e.ArchivePage)
                .ToList();
        }

        protected override bool CanMoveToFolder(IEnumerable<Page> pages)
        {
            return false;
        }
    }
}
