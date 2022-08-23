// from http://sourcechord.hatenablog.com/entry/20130303/1362315081
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace NeeLaboratory.ComponentModel
{
    /// <summary>
    /// PropetryChangedEventCollection用ユニット
    /// </summary>
    public class PropertyChangedEventItem
    {
        public PropertyChangedEventItem(INotifyPropertyChanged source, PropertyChangedEventHandler handle)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (handle is null) throw new ArgumentNullException(nameof(handle));

            Source = source;
            EventHandle = handle;
        }

        public INotifyPropertyChanged Source { get; }
        public PropertyChangedEventHandler EventHandle { get; }
    }


    /// <summary>
    /// PropertyChangedEventをまとめて登録/解除する
    /// </summary>
    public class PropetryChangedEventCollection : List<PropertyChangedEventItem>, IDisposable
    {
        private bool _disposedValue;


        public void Add(INotifyPropertyChanged source, PropertyChangedEventHandler handle)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (handle is null) throw new ArgumentNullException(nameof(handle));

            this.Add(new PropertyChangedEventItem(source, handle));
        }

        public void Add(INotifyPropertyChanged source, string? propertyName, PropertyChangedEventHandler handle)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (handle is null) throw new ArgumentNullException(nameof(handle));

            this.Add(new PropertyChangedEventItem(source, PropertyChangedTools.CreateChangedEventHandler(propertyName, handle)));
        }

        public void Regist()
        {
            ThrowIfDisposed();

            foreach (var item in this)
            {
                item.Source.PropertyChanged += item.EventHandle;
            }
        }

        public void Unregist()
        {
            ThrowIfDisposed();

            foreach (var item in this)
            {
                item.Source.PropertyChanged -= item.EventHandle;
            }
        }

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Unregist();
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
