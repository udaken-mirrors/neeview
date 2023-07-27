using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NeeView
{
    public interface IBookPageProperty : INotifyPropertyChanged, IDisposable
    {
        bool IsBusy { get; }
        ObservableCollection<Page>? PageList { get; }
        PageSortModeClass PageSortModeClass { get; }

        event EventHandler? PageListChanged;
        event EventHandler? PagesSorted;
        event EventHandler<ViewContentSourceCollectionChangedEventArgs>? ViewContentsChanged;
        event EventHandler<PageRemovedEventArgs>? PageRemoved;

        int GetMaxPageIndex();
        Page? GetPage();
        int GetPageCount();
        int GetPageIndex();
    }

}