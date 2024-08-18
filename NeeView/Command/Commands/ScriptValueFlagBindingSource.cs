using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ScriptValueFlagBindingSource : INotifyPropertyChanged, IDisposable
    {
        private string? _key;
        private bool _isChecked;
        private bool _disposedValue;
        private readonly CommandHostStaticResource _resource;

        public ScriptValueFlagBindingSource(CommandHostStaticResource resource)
        {
            _resource = resource;
            _resource.Values.CollectionChanged += Values_CollectionChanged;
        }

        private void Values_CollectionChanged(in ObservableCollections.NotifyCollectionChangedEventArgs<KeyValuePair<string, object>> e)
        {
            if (_key is null) return;

            if (e.NewItem.Key == _key)
            {
                Update();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

        public string? Key
        {
            get { return _key; }
            set
            {
                if (SetProperty(ref _key, value))
                {
                    Update();
                }
            }
        }


        private void Update()
        {
            if (_key is null)
            {
                IsChecked = false;
            }
            else if (_resource.Values.TryGetValue(_key, out var value))
            {
                IsChecked = value is bool b && b;
            }
            else
            {
                IsChecked = false;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _resource.Values.CollectionChanged -= Values_CollectionChanged;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
