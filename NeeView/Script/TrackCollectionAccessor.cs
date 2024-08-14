using System;
using System.Linq;

namespace NeeView
{
    public class TrackCollectionAccessor
    {
        private readonly TrackCollection _collection;

        public TrackCollectionAccessor(TrackCollection collection)
        {
            _collection = collection;
        }

        [WordNodeMember]
        public string[] Tracks => _collection.Tracks.Select(e => e.Name).ToArray();

        [WordNodeMember]
        public int SelectedIndex
        {
            get
            {
                if (_collection.Selected is null) return -1;
                return _collection.Tracks.IndexOf(_collection.Selected);
            }
            set
            {
                AppDispatcher.BeginInvoke(() => _collection.Selected = _collection.Tracks[Math.Clamp(value, 0, _collection.Tracks.Count - 1)]);
            }
        }

    }
}
