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
    public class BookmarkPanel : BindableBase, IPanel
    {
        private readonly BookmarkListView _view;
        private readonly BookmarkFolderListPresenter _presenter;

        public BookmarkPanel(BookmarkFolderList folderList)
        {
            _view = new BookmarkListView(folderList);
            _presenter = new BookmarkFolderListPresenter(_view, folderList);

            Icon = App.Current.MainWindow.Resources["pic_star_24px"] as DrawingImage ?? throw new InvalidOperationException("Cannot found resource `pic_star_24px`");
        }

#pragma warning disable CS0067
        public event EventHandler? IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(BookmarkPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => Properties.TextResources.GetString("Bookmark.Title");

        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;

        public BookmarkFolderListPresenter Presenter => _presenter;


        public void Refresh()
        {
            _presenter.Refresh();
        }

        public void Focus()
        {
            _presenter.FocusAtOnce();
        }
    }

}
