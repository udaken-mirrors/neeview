using System.Windows.Controls;

namespace NeeView
{
    public class PageListItemCommandResource : PageCommandResource<Page>
    {
        public PageListItemCommandResource(IToolTipService? toolTipService) : base(toolTipService)
        {
        }

        protected override ListBoxItemRenamer<Page>? CreateListBoxItemRenamer(ListBox listBox, Page item, IToolTipService? toolTipService)
        {
            var renamer = new PageListItemRenamer(listBox, toolTipService, true);
            renamer.SelectedItemChanged += (s, e) => BookOperation.Current.JumpPage(this, item);
            renamer.ArchiveChanged += (s, e) => BookOperation.Current.BookControl.ReLoad();
            return renamer;
        }
    }
}
