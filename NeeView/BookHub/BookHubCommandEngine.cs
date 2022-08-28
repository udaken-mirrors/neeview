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
    /// BookHubコマンド引数基底
    /// </summary>
    public class BookHubCommandArgs
    {
    }

    /// <summary>
    /// BookHubコマンド基底
    /// </summary>
    public abstract class BookHubCommand : JobBase
    {
        protected BookHub _bookHub { get; private set; }


        public BookHubCommand(BookHub bookHub)
        {
            _bookHub = bookHub;
        }

        public bool CanBeCanceled { get; set; } = true;
    }

    /// <summary>
    /// CommandLoad 引数
    /// </summary>
    public class BookHubCommandLoadArgs : BookHubCommandArgs
    {
        public BookHubCommandLoadArgs(string path, string sourcePath)
        {
            Debug.Assert(path is not null);
            Debug.Assert(sourcePath is not null);

            Path = path;
            SourcePath = sourcePath;
        }

        public object? Sender { get; set; }
        public string Path { get; set; }
        public string SourcePath { get; set; }
        public string? StartEntry { get; set; }
        public BookLoadOption Option { get; set; }
        public bool IsRefreshFolderList { get; set; }
    }

    /// <summary>
    /// CommandLoad
    /// </summary>
    public class BookHubCommandLoad : BookHubCommand
    {
        private BookHubCommandLoadArgs _param;

        public string Path => _param.Path;

        public BookHubCommandLoad(BookHub bookHub, BookHubCommandLoadArgs param) : base(bookHub)
        {
            _param = param;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _bookHub.LoadAsync(_param, token);
        }
    }


    /// <summary>
    /// CommandUnload引数
    /// </summary>
    public class BookHubCommandUnloadArgs : BookHubCommandArgs
    {
        public object? Sender { get; set; }
        public bool IsClearViewContent { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// CommandUnload
    /// </summary>
    public class BookHubCommandUnload : BookHubCommand
    {
        private BookHubCommandUnloadArgs _param;

        public BookHubCommandUnload(BookHub bookHub, BookHubCommandUnloadArgs param) : base(bookHub)
        {
            _param = param;

            // キャンセル不可
            this.CanBeCanceled = false;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            _bookHub.Unload(_param);

            // ブックを閉じたときの移動履歴を表示するためにnullを履歴に登録
            BookHubHistory.Current.Add(_param.Sender, null);

            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// BookHub用コマンドエンジン
    /// </summary>
    public class BookHubCommandEngine : SingleJobEngine
    {
        public BookHubCommandEngine() : base(nameof(BookHubCommandEngine))
        {
        }

        public BookHubCommandEngine(string name) : base(name)
        {
        }


        protected override Queue<IJob> Enqueue(IJob job, Queue<IJob> queue)
        {
            if (job is not BookHubCommand) throw new ArgumentException("job must be BookHubCommand");
            if (queue is null) throw new ArgumentNullException(nameof(queue));

            // 全コマンドキャンセル
            // ※ Unloadはキャンセルできないので残る
            foreach (var e in AllJobs().OfType<BookHubCommand>().Where(e => e.CanBeCanceled))
            {
                e.Cancel();
            }

            return base.Enqueue(job, queue);
        }
    }
}
