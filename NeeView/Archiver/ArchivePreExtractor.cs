//#define LOCAL_DEBUG

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 事前展開
    /// </summary>
    public partial class ArchivePreExtractor : IDisposable
    {
        private readonly Archiver _archiver;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private ArchivePreExtractState _state;
        private TempDirectory? _extractDirectory;
        private bool _disposedValue;


        public ArchivePreExtractor(Archiver archiver)
        {
            _archiver = archiver;
            _state = ArchivePreExtractState.None;
        }


        [Subscribable]
        public event EventHandler<PreExtractStateChangedEventArgs>? StateChanged;


        public ArchivePreExtractState State
        {
            get { return _state; }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    Trace($"State = {value}");
                    StateChanged?.Invoke(this, new PreExtractStateChangedEventArgs(value));
                }
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                _cancellationTokenSource.Cancel();
                _disposedValue = true;
            }
        }

        ~ArchivePreExtractor()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// 可能であれば状態を初期化する
        /// </summary>
        private void ResetState()
        {
            if (State.IsReady())
            {
                State = ArchivePreExtractState.None;
            }
        }

        // TODO: async? 7z の solid 判定は非同期化する必要あるかも？
        private bool CanPreExtract()
        {
            return _archiver.CanPreExtract();
        }

        private async Task PreExtractAsync(CancellationToken token)
        {
            Debug.Assert(CanPreExtract());
            if (!State.IsReady()) return;

            State = ArchivePreExtractState.Extracting;
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, _cancellationTokenSource.Token);

            try
            {
                var sw = Stopwatch.StartNew();
                Trace($"PreExtract ...");
                if (_extractDirectory is null)
                {
                    var directory = Temporary.Current.CreateCountedTempFileName("arc", "");
                    Directory.CreateDirectory(directory);
                    _extractDirectory = new TempDirectory(directory);
                    Trace($"PreExtract create directory. {sw.ElapsedMilliseconds}ms");
                }

                await _archiver.PreExtractAsync(_extractDirectory.Path, linked.Token);
                sw.Stop();
                Trace($"PreExtract done. {sw.ElapsedMilliseconds}ms");
                State = ArchivePreExtractState.Done;
            }
            catch (OperationCanceledException)
            {
                State = ArchivePreExtractState.Canceled;
            }
            catch (Exception)
            {
                State = ArchivePreExtractState.Failed;
            }
        }

        /// <summary>
        /// エントリーの事前展開完了を待機
        /// </summary>
        /// <remarks>
        /// 事前展開開始も行う
        /// </remarks>
        /// <param name="entry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task WaitPreExtractAsync(ArchiveEntry entry, CancellationToken token)
        {
            if (!CanPreExtract()) return;
            if (entry.Data is not null) return;

            // キャンセル状態を初期状態に戻す
            ResetState();

            // 事前展開を非同期に実行
            // TODO: 事前展開のキャンセルリクエストはエントリ要求とは別
            _ = PreExtractAsync(CancellationToken.None);

            Trace($"WaitPreExtract: {entry} ...");

            // wait for entry.Data changed
            using var disposables = new DisposableCollection();
            var tcs = new TaskCompletionSource();
            disposables.Add(token.Register(() => tcs.TrySetCanceled()));
            disposables.Add(entry.SubscribeDataChanged((s, e) => tcs.TrySetResult()));
            disposables.Add(this.SubscribeStateChanged((s, e) => { if (e.State.IsCompleted()) tcs.TrySetResult(); }));
            if (entry.Data is null && !State.IsCompleted())
            {
                await tcs.Task;
            }

            Trace($"WaitPreExtract: {entry} done.");
        }


        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s)
        {
            Debug.WriteLine($"{this.GetType().Name}: {s}");
        }

        [Conditional("LOCAL_DEBUG")]
        private void Trace(string s, params object[] args)
        {
            Debug.WriteLine($"{this.GetType().Name}: {string.Format(s, args)}");
        }
    }


    public class PreExtractStateChangedEventArgs(ArchivePreExtractState state) : EventArgs
    {
        public ArchivePreExtractState State { get; } = state;
    }
}

