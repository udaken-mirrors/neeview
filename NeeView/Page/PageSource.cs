using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NeeLaboratory.Threading;
using NeeView.ComponentModel;
using NeeView.Threading;

namespace NeeView
{
    /// <summary>
    /// PageSource
    ///   +- MemoryPageSource
    ///   +- FilePageSource
    /// </summary>
    public abstract class PageSource<T> : IDataSource<T>, IDataSource
    {
        private readonly AsyncLock _asyncLock = new();
        private CancellationTokenSource? _cancellationTokenSource;

        private ArchiveEntry _archvieEntry;


        public PageSource(ArchiveEntry entry)
        {
            _archvieEntry = entry;
        }


        public event EventHandler? SourceChanged;


        public ArchiveEntry ArchiveEntry => _archvieEntry;

        public T? Data { get; protected set; }

        public string? ErrorMessage { get; protected set; }

        public bool IsLoaded => Data != null || IsFailed;

        public bool IsFailed => ErrorMessage != null;

        object? IDataSource.Data => Data;

        public abstract long DataSize { get; }



        public async Task LoadAsync(CancellationToken token)
        {
            using (await _asyncLock.LockAsync(token))
            {
                if (IsLoaded)
                {
                    return;
                }

                try
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                    using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, token);
                    if (linkedTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    await LoadAsyncCore(linkedTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }

        protected abstract Task LoadAsyncCore(CancellationToken token);


        public void Unload()
        {
            _cancellationTokenSource?.Cancel();
            UnloadCore();
        }

        protected abstract void UnloadCore();


        protected void SetData(T? data, string? errorMessage)
        {
            bool isContentChanged = !EqualityComparer<T>.Default.Equals(Data, data) || ErrorMessage != errorMessage;

            Data = data;
            ErrorMessage = errorMessage;

            if (isContentChanged)
            {
                Debug.WriteLine($"PageSourceChanged: {ArchiveEntry}");
                SourceChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

}
