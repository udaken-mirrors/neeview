using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FolderPanel : BindableBase, IPanel
    {
        private readonly FolderListView _view;
        private readonly BookshelfFolderListPresenter _folderListPresenter;

        public FolderPanel(BookshelfFolderList folderList)
        {
            _view = new FolderListView(folderList);
            _folderListPresenter = new BookshelfFolderListPresenter(_view, folderList);

            Icon = App.Current.MainWindow.Resources["pic_bookshelf"] as DrawingImage
                ?? throw new InvalidOperationException("cannot found resource 'pic_bookshelf'");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(FolderPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => Properties.TextResources.GetString("Bookshelf.Title");

        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;

        public BookshelfFolderListPresenter Presenter => _folderListPresenter;

        public void Refresh()
        {
            _folderListPresenter.Refresh();
        }

        public void Focus()
        {
            _folderListPresenter.FocusAtOnce();
        }
    }

}
