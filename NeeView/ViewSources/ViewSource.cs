using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;

namespace NeeView
{

    public abstract partial class ViewSource : IDataSource, IMemoryElement
    {
        private IPageContent _pageContent;
        private BookMemoryService _bookMemoryService;
        private DataSource _data = new();
        private object _lock = new();

        protected ViewSource(IPageContent pageContent, BookMemoryService bookMemoryService)
        {
            _pageContent = pageContent;
            _bookMemoryService = bookMemoryService;
        }

        [Subscribable]
        public event EventHandler<DataSourceChangedEventArgs>? DataSourceChanged;

        public DataSource DataSource => _data;
        public object? Data => _data.Data;
        public long DataSize => _data.DataSize;
        public string? ErrorMessage => _data.ErrorMessage;
        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;

        public int Index => _pageContent.Index;

        // TODO: PageContent に IMemoryElement を継承？
        public virtual bool IsMemoryLocked => (_pageContent as IMemoryElement)?.IsMemoryLocked ?? false;

        public long GetMemorySize() => DataSize;


        public async Task LoadAsync(Size size, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var data = _pageContent.CreateDataSource();

            if (data.IsFailed)
            {
                SetData(null, 0, _pageContent.ErrorMessage);
                return;
            }

            if (data.Data is null) return;

            try
            {
                await LoadCoreAsync(data, size, token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                SetData(null, 0, ex.Message);
            }
        }

        public abstract Task LoadCoreAsync(DataSource data, Size size, CancellationToken token);

        public abstract void Unload();

        protected void SetData(object? data, long dataSize, string? errorMessage)
        {
            SetData(new DataSource(data, dataSize, errorMessage));
        }

        protected void SetData(DataSource data)
        {
            lock (_lock)
            {
                if (_data != data)
                {
                    _data = data;
                    if (_data.DataSize > 0)
                    {
                        _bookMemoryService.AddPictureSource(this);
                    }
                    DataSourceChanged?.Invoke(this, new DataSourceChangedEventArgs(_data));
                }
            }
        }
    }

}
