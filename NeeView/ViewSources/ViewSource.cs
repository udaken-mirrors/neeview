using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using NeeLaboratory.Generators;
using NeeView.ComponentModel;
using NeeView.Threading;

namespace NeeView
{
    [NotifyPropertyChanged]
    public partial class ViewSource : IDataSource, IMemoryElement, IHasImageSource, INotifyPropertyChanged
    {
        private readonly PageContent _pageContent;
        private readonly BookMemoryService _bookMemoryService;
        private DataSource _data = new();
        private readonly object _lock = new();
        private readonly AsyncLock _asyncLock = new();
        private IViewSourceStrategy? _strategy;


        public ViewSource(PageContent pageContent, BookMemoryService bookMemoryService)
        {
            _pageContent = pageContent;
            _bookMemoryService = bookMemoryService;
        }


        [Subscribable]
        public event EventHandler<DataSourceChangedEventArgs>? DataSourceChanged;

        [Subscribable]
        public event PropertyChangedEventHandler? PropertyChanged;



        public DataSource DataSource
        {
            get { return _data; }
            set
            {
                if (SetProperty(ref _data, value))
                {
                    if (_data.DataSize > 0)
                    {
                        _bookMemoryService.AddPictureSource(this);
                    }
                    DataSourceChanged?.Invoke(this, new DataSourceChangedEventArgs(_data));

                    RaisePropertyChanged(nameof(Data));
                    RaisePropertyChanged(nameof(DataSize));
                    RaisePropertyChanged(nameof(ErrorMessage));
                    RaisePropertyChanged(nameof(IsLoaded));
                    RaisePropertyChanged(nameof(IsFailed));
                }
            }
        }

        public object? Data => _data.Data;
        public long DataSize => _data.DataSize;

        public string? ErrorMessage => _data.ErrorMessage;
        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;

        public int Index => _pageContent.Index;

        // TODO: PageContent に IMemoryElement を継承？
        public virtual bool IsMemoryLocked => (_pageContent as IMemoryElement)?.IsMemoryLocked ?? false;

        public long GetMemorySize() => DataSize;


        public ImageSource? ImageSource => (Data as ImageSource) ?? (Data as IHasImageSource)?.ImageSource;


        /// <summary>
        /// 求める ViewSource データ生成済かをチェック
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <returns>生成済？</returns>
        private bool CheckLoaded(Size size)
        {
            return IsLoaded && _strategy?.CheckLoaded(size) == true;
        }

        /// <summary>
        /// ViewSource データ生成
        /// </summary>
        /// <param name="size">サイズ</param>
        /// <param name="token">キャンセルトークン</param>
        public async Task LoadAsync(Size size, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using (await _asyncLock.LockAsync(token))
            {
                // ロード済であれば何もしない
                if (CheckLoaded(size))
                {
                    return;
                }

                // TODO: dataを返したほうが堅牢だね？
                await _pageContent.LoadAsync(token);

                var data = _pageContent.CreateDataSource();
                if (data.IsFailed)
                {
                    SetData(null, 0, _pageContent.ErrorMessage);
                    return;
                }

                // NOTE: エラーなく読み込んだのにデータが無いとかありえないが念のため
                if (data.Data is null)
                {
                    SetData(null, 0, "InvalidOperationException: cannot load data");
                    return;
                }

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
        }

        private async Task LoadCoreAsync(DataSource data, Size size, CancellationToken token)
        {
            NVDebug.AssertMTA();

            if (data.IsFailed)
            {
                SetData(null, 0, data.ErrorMessage);
            }
            else
            {
                if (_strategy is null)
                {
                    _strategy = ViewSourceStrategyFactory.Create(_pageContent, data);
                }

                if (_strategy is not null)
                {
                    SetData(await _strategy.LoadCoreAsync(data, size, token));
                }
                else
                {
                    SetData(null, 0, $"InvalidOperationException: No ViewSourceStrategy");
                }
            }
        }

        public void Unload()
        {
            if (Data is not null)
            {
                _strategy?.Unload();
                SetData(null, 0, null);
            }
        }


        private void SetData(object? data, long dataSize, string? errorMessage)
        {
            SetData(new DataSource(data, dataSize, errorMessage));
        }

        private void SetData(DataSource data)
        {
            lock (_lock)
            {
                DataSource = data;
#if false
                if (_data != data)
                {
                    _data = data;
                    if (_data.DataSize > 0)
                    {
                        _bookMemoryService.AddPictureSource(this);
                    }
                    DataSourceChanged?.Invoke(this, new DataSourceChangedEventArgs(_data));
                }
#endif
            }
        }
    }

}
