using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NeeView
{
    public interface IBookPageProperty : INotifyPropertyChanged, IDisposable
    {
        event EventHandler? PageListChanged;
        event EventHandler<ViewContentSourceCollectionChangedEventArgs>? ViewContentsChanged;
        public event EventHandler<SelectedRangeChangedEventArgs>? SelectedItemChanged;

        bool IsBusy { get; }
        IReadOnlyList<Page>? PageList { get; }
        PageSortModeClass PageSortModeClass { get; }
        public int SelectedIndex { get; set; }
        public int MaxIndex { get; }


        int GetMaxPageIndex();
        Page? GetPage();
        int GetPageCount();
        int GetPageIndex();
    }

}