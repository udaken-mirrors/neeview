using System;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public interface IHasFolderListBox
    {
        void SetFolderListBoxContent(FolderListBox content);
    }

    public class FolderListPresenter
    {
        private readonly FolderList _folderList;
        private readonly FolderListBoxViewModel _folderListBoxViewModel;
        private IHasFolderListBox? _folderListView;
        private FolderListBox? _folderListBox;


        public FolderListPresenter(FolderList folderList)
        {
            _folderList = folderList;
            _folderList.FolderListConfig.AddPropertyChanged(nameof(FolderListConfig.PanelListItemStyle), (s, e) => UpdateFolderListBox());

            _folderListBoxViewModel = new FolderListBoxViewModel(folderList);
            UpdateFolderListBox();
        }


        public FolderListBox? FolderListBox => _folderListBox;


        public void InitializeView(IHasFolderListBox folderListView)
        {
            _folderListView = folderListView;
            UpdateFolderListBox(false);
        }

        public void UpdateFolderListBox(bool rebuild = true)
        {
            if (rebuild || _folderListBox is null)
            {
                _folderListBox = new FolderListBox(_folderListBoxViewModel);
            }
            _folderListView?.SetFolderListBoxContent(_folderListBox);
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
