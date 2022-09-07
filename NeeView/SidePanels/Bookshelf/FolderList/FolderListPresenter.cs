using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NeeView
{
    public interface IHasFolderListBox
    {
        void SetFolderListBoxContent(FolderListBox content);
    }

    public class FolderListPresenter
    {
        private readonly IHasFolderListBox _folderListView;
        private readonly FolderList _folderList;
        private readonly FolderListBoxViewModel _folderListBoxViewModel;
        private FolderListBox? _folderListBox;


        public FolderListPresenter(IHasFolderListBox folderListView, FolderList folderList)
        {
            _folderListView = folderListView;
            _folderList = folderList;
            _folderList.FolderListConfig.AddPropertyChanged(nameof(FolderListConfig.PanelListItemStyle), (s, e) => UpdateFolderListBox());

            _folderListBoxViewModel = new FolderListBoxViewModel(folderList);
            UpdateFolderListBox();
        }


        public FolderListBox? FolderListBox => _folderListBox;


        public void UpdateFolderListBox()
        {
            _folderListBox = new FolderListBox(_folderListBoxViewModel);
            _folderListView.SetFolderListBoxContent(_folderListBox);
        }

        public void Refresh()
        {
            _folderListBox?.Refresh();
        }

        public void FocusAtOnce()
        {
            _folderList.FocusAtOnce();
            _folderListBox?.FocusSelectedItem(false);
        }
    }
}
