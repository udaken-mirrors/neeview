using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Vlc.DotNet.Core;

namespace NeeView
{
    public class VlcTrackCollectionSource : IDisposable
    {
        private readonly IEnumerableManagement<TrackDescription> _source;
        private readonly TrackCollection _collection;
        private bool _disposedValue;
        private readonly DisposableCollection _disposables = new();
        private bool _isEnabled = true;


        public VlcTrackCollectionSource(IEnumerableManagement<TrackDescription> source)
        {
            _source = source;
            _collection = new TrackCollection(source.All.Select(e => new TrackItem(e.ID, e.Name)));

            var selectedId = _source.Current.ID;
            _collection.Selected = _collection.Tracks.FirstOrDefault(e => e.ID == selectedId);

            Debug.Assert(_collection.Selected is not null);
            Debug.Assert(_collection.Tracks.Contains(_collection.Selected));

            _disposables.Add(_collection.SubscribePropertyChanged(nameof(_collection.Selected), Collection_SelectedChanged));
        }


        public TrackCollection Collection => _collection;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if (_disposedValue) return;
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    UpdateCurrent();
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposables.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Collection_SelectedChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateCurrent();
        }

        public void UpdateCurrent()
        {
            if (_disposedValue) return;

            var selected = _collection.Selected;
            if (selected is not null)
            {
                Task.Run(() =>
                {
                    if (_disposedValue) return;
                    try
                    {
                        if (_isEnabled)
                        {
                            _source.Current = _source.All.FirstOrDefault(x => x.ID == selected.ID) ?? _source.Current;
                        }
                        else
                        {
                            _source.Current = _source.All.FirstOrDefault() ?? _source.Current;
                        }
                    }
                    catch
                    {
                        // nop.
                    }
                });
            }
        }
    }
}

