using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using NeeLaboratory.Threading.Jobs;

namespace NeeView
{
    /// <summary>
    /// Bookコマンド基底
    /// </summary>
    internal abstract class BookCommand : JobBase
    {
        public BookCommand(object? sender, int priority)
        {
            _sender = sender;
            Priority = priority;
        }

        /// <summary>
        /// 送信者
        /// </summary>
        protected object? _sender;

        /// <summary>
        /// コマンド優先度
        /// </summary>
        public int Priority { get; private set; }


        protected sealed override async Task ExecuteAsync(CancellationToken token)
        {
            ////Book.Log.TraceEvent(TraceEventType.Information, 0, $"{this} ...");
            await OnExecuteAsync(token);
            ////Book.Log.TraceEvent(TraceEventType.Information, 0, $"{this} done.");
        }

        protected abstract Task OnExecuteAsync(CancellationToken token);

        protected override void OnCanceled()
        {
            ////Book.Log.TraceEvent(TraceEventType.Information, 0, $"{this} canceled.");
        }

        protected override void OnException(Exception e)
        {
            ////Book.Log.TraceEvent(TraceEventType.Error, 0, $"{this} exception: {e.Message}\n{e.StackTrace}");
            ////Book.Log.Flush();
        }
    }



    /// <summary>
    /// 一般コマンド
    /// </summary>
    internal class BookCommandAction : BookCommand
    {
        private readonly Func<object?, CancellationToken, Task> _taskAction;

        public BookCommandAction(object? sender, Func<object?, CancellationToken, Task> taskAction, int priority) : base(sender, priority)
        {
            _taskAction = taskAction;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _taskAction(_sender, token);
        }
    }

    /// <summary>
    /// キャンセル可能コマンド
    /// </summary>
    internal class BookCommandCancellableAction : BookCommand
    {
        private readonly Func<object?, CancellationToken, Task> _taskAction;
        private CancellationToken _cancelToken;

        public BookCommandCancellableAction(object? sender, Func<object?, CancellationToken, Task> taskAction, int priority, CancellationToken cancelToken) : base(sender, priority)
        {
            _taskAction = taskAction;
            _cancelToken = cancelToken;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _cancelToken);
            tokenSource.Token.ThrowIfCancellationRequested();
            await _taskAction(_sender, tokenSource.Token);
        }
    }


    /// <summary>
    /// Bookコマンドエンジン
    /// </summary>
    internal class BookCommandEngine : SingleJobEngine
    {
        public BookCommandEngine() : base(nameof(BookCommandEngine))
        {
        }
    }
}
