using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class HistoryListBoxViewModel : INotifyPropertyChanged
    {
        private readonly HistoryList _model;
        private Visibility _visibility = Visibility.Hidden;


        public HistoryListBoxViewModel(HistoryList model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(HistoryList.Items),
                (s, e) => RaisePropertyChanged(nameof(Items)));

            _model.AddPropertyChanged(nameof(HistoryList.SelectedItem),
                (s, e) => RaisePropertyChanged(nameof(SelectedItem)));
        }


        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;



        public bool IsThumbnailVisible => _model.IsThumbnailVisible;

        public List<BookHistory> Items => _model.Items;

        public BookHistory? SelectedItem
        {
            get => _model.SelectedItem;
            set => _model.SelectedItem = value;
        }

        public Visibility Visibility
        {
            get { return _visibility; }
            set
            {
                if (SetProperty(ref _visibility, value))
                {
                    _model.IsEnabled = _visibility == Visibility.Visible;
                }
            }
        }

        public async Task UpdateItemsAsync(CancellationToken token)
        {
            await _model.UpdateItemsAsync(token);
        }

        public void UpdateItems(bool force, CancellationToken token)
        {
            _model.UpdateItems(force, token);
        }

        public void Remove(IEnumerable<BookHistory> items)
        {
            _model.Remove(items);
        }

        public void Load(string path)
        {
            if (path == null) return;
            BookHub.Current?.RequestLoad(this, path, null, BookLoadOption.KeepHistoryOrder | BookLoadOption.SkipSamePlace | BookLoadOption.IsBook, true);
        }

        public bool IsLRKeyEnabled()
        {
            return Config.Current.Panels.IsLeftRightKeyEnabled || _model.PanelListItemStyle == PanelListItemStyle.Thumbnail;
        }
    }
}
