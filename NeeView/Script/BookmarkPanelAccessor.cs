using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class BookmarkPanelAccessor : LayoutPanelAccessor
    {
        private readonly BookmarkPanel _panel;
        private readonly BookmarkFolderList _model;


        public BookmarkPanelAccessor() : base(nameof(BookmarkPanel))
        {
            _panel = (BookmarkPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(BookmarkPanel));
            _model = _panel.Presenter.BookmarkFolderList;

            FolderTree = new BookmarkFolderTreeAccessor(BookmarkFolderTreeModel.Current ?? throw new InvalidOperationException());
        }


        [WordNodeMember(IsAutoCollect = false)]
        public BookmarkFolderTreeAccessor FolderTree { get; }

        [WordNodeMember]
        public string? Path
        {
            get { return _model.Place?.SimplePath; }
            set { AppDispatcher.Invoke(() => _model.RequestPlace(new QueryPath(value), null, FolderSetPlaceOption.UpdateHistory)); }
        }

        [WordNodeMember]
        public string SearchWord
        {
            get { return AppDispatcher.Invoke(() => _panel.Presenter.BookmarkListView.GetSearchBoxText()); }
            set { AppDispatcher.Invoke(() => _panel.Presenter.BookmarkListView.SetSearchBoxText(value)); }
        }

        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return _model.FolderListConfig.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.FolderListConfig.PanelListItemStyle = value.ToEnum<PanelListItemStyle>()); }
        }

        [WordNodeMember(DocumentType = typeof(FolderOrder))]
        public string FolderOrder
        {
            get { return _model.GetFolderOrder().ToString(); }
            set { AppDispatcher.Invoke(() => _model.SetFolderOrder(value.ToEnum<FolderOrder>())); }
        }

        [WordNodeMember]
        public BookmarkItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public BookmarkItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }


        private BookmarkItemAccessor[] GetItems()
        {
            return ToItemAccessorArray(_panel.Presenter.FolderListBox?.GetItems());
        }

        private BookmarkItemAccessor[] GetSelectedItems()
        {
            return ToItemAccessorArray(_panel.Presenter.FolderListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(BookmarkItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? Array.Empty<BookmarkItemAccessor>();
            _panel.Presenter.FolderListBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private static BookmarkItemAccessor[] ToItemAccessorArray(IEnumerable<FolderItem>? items)
        {
            return items?.Select(e => new BookmarkItemAccessor(e)).ToArray() ?? Array.Empty<BookmarkItemAccessor>();
        }

        [WordNodeMember]
        public void NewFolder(string? name)
        {
            AppDispatcher.Invoke(() => _model.NewFolder(name));
        }


        internal WordNode CreateWordNode(string name)
        {
            var node = WordNodeHelper.CreateClassWordNode(name, this.GetType());
            node.Children?.Add(FolderTree.CreateWordNode(nameof(FolderTree)));
            return node;
        }
    }
}
