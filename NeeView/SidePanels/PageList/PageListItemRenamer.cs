using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// ページリスト用名前変更機能
    /// </summary>
    public class PageListItemRenamer : ListBoxItemRenamer<Page>
    {
        public PageListItemRenamer(ListBox listBox, IToolTipService? toolTipService) : base(listBox, toolTipService)
        {
        }

        public event EventHandler? ArchiveChanged;

        protected override RenameControl CreateRenameControl(ListBox listBox, Page item)
        {
            var control = new PageListItemRenameControl(listBox, item);
            control.ArchiveChanged += (s, e) => ArchiveChanged?.Invoke(s, e);
            return control;
        }
    }

    public class PageListItemRenameControl : ListBoxItemRenameControl<Page>
    {
        private readonly bool _isFileSystem;

        public PageListItemRenameControl(ListBox listBox, Page item) : base(listBox, item)
        {
            if (item.Entry.IsFileSystem)
            {
                this.IsSeleftFileNameBody = !item.Entry.IsDirectory;
                this.IsInvalidFileNameChars = true;
                _isFileSystem = true;
            }
            else
            {
                this.IsInvalidSeparatorChars = true;
                _isFileSystem = false;

            }
        }

        public event EventHandler? ArchiveChanged;

        protected override async Task<bool> OnRenameAsync(string oldValue, string newValue)
        {
            Debug.Assert(oldValue != newValue);
            var isSuccess = await _item.RenameAsync(newValue);
            if (isSuccess && !_isFileSystem)
            {
                ArchiveChanged?.Invoke(this, EventArgs.Empty);
            }
            return isSuccess;
        }
    }
}
