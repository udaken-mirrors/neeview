using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace NeeView
{
    public class BookPagePropertyProxy : BindableBase, IBookPageProperty, IDisposable
    {
        private IBookPageProperty? _source;
        private bool _disposedValue;

        public BookPagePropertyProxy()
        {
        }

        public event EventHandler? PageListChanged;
        public event EventHandler<ViewContentSourceCollectionChangedEventArgs>? ViewContentsChanged;
        public event EventHandler<SelectedRangeChangedEventArgs>? SelectedItemChanged;


        public bool IsBusy => _source?.IsBusy ?? false;

        public IReadOnlyList<Page>? PageList => _source?.PageList ?? null;

        public PageSortModeClass PageSortModeClass => _source?.PageSortModeClass ?? PageSortModeClass.Full;

        public int SelectedIndex
        {
            get { return _source?.SelectedIndex ?? 0; }
            set { if (_source is not null) _source.SelectedIndex = value; }
        }

        public int MaxIndex => _source?.MaxIndex ?? 0;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Detach();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetSource(IBookPageProperty? source)
        {
            if (_source == source) return;

            Detach();
            Attach(source);

            RaisePropertyChanged(nameof(IsBusy));
            RaisePropertyChanged(nameof(PageList));
            RaisePropertyChanged(nameof(PageSortModeClass));
            RaisePropertyChanged(nameof(SelectedIndex));
            RaisePropertyChanged(nameof(MaxIndex));
            PageListChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Attach(IBookPageProperty? source)
        {
            Debug.Assert(_source is null);

            _source = source;
            if (_source is null) return;

            _source.PropertyChanged += Source_PropertyChanged;
            _source.PageListChanged += Source_PageListChanged;
            _source.ViewContentsChanged += Source_ViewContentsChanged;
            _source.SelectedItemChanged += Source_SelectedItemChanged;
        }


        private void Detach()
        {
            if (_source is null) return;

            _source.PropertyChanged -= Source_PropertyChanged;
            _source.PageListChanged -= Source_PageListChanged;
            _source.ViewContentsChanged -= Source_ViewContentsChanged;
            _source.SelectedItemChanged -= Source_SelectedItemChanged;
            _source.Dispose();
            _source = null;
        }

        private void Source_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        private void Source_PageListChanged(object? sender, EventArgs e)
        {
            PageListChanged?.Invoke(sender, e);
        }

        private void Source_ViewContentsChanged(object? sender, ViewContentSourceCollectionChangedEventArgs e)
        {
            ViewContentsChanged?.Invoke(sender, e);
        }

        private void Source_SelectedItemChanged(object? sender, SelectedRangeChangedEventArgs e)
        {
            SelectedItemChanged?.Invoke(sender, e);
        }


        public int GetMaxPageIndex()
        {
            return _source?.GetMaxPageIndex() ?? 0;
        }

        public Page? GetPage()
        {
            return _source?.GetPage() ?? null;
        }

        public int GetPageCount()
        {
            return _source?.GetPageCount() ?? 0;
        }

        public int GetPageIndex()
        {
            return _source?.GetPageIndex() ?? 0;
        }
    }
}