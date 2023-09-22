using NeeLaboratory.Generators;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class TrackCollection : INotifyPropertyChanged
    {
        public List<TrackItem> _items;
        private TrackItem? _selected;

        public TrackCollection(IEnumerable<TrackItem> items)
        {
            _items = new List<TrackItem>(items);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public List<TrackItem> Tracks => _items;

        public TrackItem? Selected
        {
            get { return _selected; }
            set
            {
                SetProperty(ref _selected, value);
            }
        }
    }
}

