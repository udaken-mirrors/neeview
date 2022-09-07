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
    /// 結合コマンド
    /// </summary>
    internal class BookCommandJoinAction : BookCommand
    {
        private readonly Func<object?, int, CancellationToken, Task> _taskAction;
        private int _value;

        public BookCommandJoinAction(object? sender, Func<object?, int, CancellationToken, Task> taskAction, int value, int priority) : base(sender, priority)
        {
            _taskAction = taskAction;
            _value = value;
        }

        protected override async Task OnExecuteAsync(CancellationToken token)
        {
            await _taskAction(_sender, _value, token);
        }

        public void Join(BookCommandJoinAction other)
        {
            _value += other._value;
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

        public BookCommandEngine(string name) : base(name)
        {
        }


        protected override Queue<IJob> Enqueue(IJob job, Queue<IJob> queue)
        {
            Debug.Assert(job is BookCommand);
            Debug.Assert(queue is not null);

            if (job is not BookCommand request) return queue;

            Debug.Assert(queue.Count <= 1);
            var select = queue.Count > 0 ? (BookCommand)queue.Peek() : null;

            if (select is null)
            {
                select = request;
            }
            // TODO: たまたまBookCommandJoinActionを使っている命令が一種類だけだったのでうまくいっているだけ。要修正
            else if (BookProfile.Current.CanMultiplePageMove()
                && request is BookCommandJoinAction requestJoinable
                && select is BookCommandJoinAction selectJoinable)
            {
                selectJoinable.Join(requestJoinable);
            }
            else
            {
                select = request.Priority >= select.Priority ? request : select;
            }

            queue.Clear();
            queue.Enqueue(select);

            return queue;
        }

    }
}
