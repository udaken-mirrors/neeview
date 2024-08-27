using NeeLaboratory.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;


namespace NeeView
{
    public class FileInformationItemCommandResource : PageCommandResource<FileInformationSource>
    {
        protected override Page? GetSelectedPage(object sender)
        {
            if (sender is not ListBox listBox) return null;

            var page = (listBox.SelectedItem as FileInformationSource)?.Page;
            if (page is null) return null;
            return page.PageType != PageType.Empty ? page : null;
        }

        protected override List<Page>? GetSelectedPages(object sender)
        {
            if (sender is not ListBox listBox) return null;

            return listBox.SelectedItems
                .Cast<FileInformationSource>()
                .Select(e => e.Page)
                .Where(e => e.PageType != PageType.Empty)
                .WhereNotNull()
                .ToList();
        }

        protected override ListBoxItemRenamer<FileInformationSource>? CreateListBoxItemRenamer(ListBox listBox, FileInformationSource item, IToolTipService? toolTipService)
        {
            var renamer = new FileInformationItemRenamer(listBox, toolTipService);
            renamer.SelectedItemChanged += (s, e) => BookOperation.Current.JumpPage(this, item.Page);
            renamer.ArchiveChanged += (s, e) => BookOperation.Current.BookControl.ReLoad();
            return renamer;
        }
    }

}
