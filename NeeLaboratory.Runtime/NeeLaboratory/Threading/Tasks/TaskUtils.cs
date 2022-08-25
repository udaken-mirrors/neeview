using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.Threading.Tasks
{
    public static class TaskUtils
    {
        // なんだこれ。
        public static Task ActionAsync(Action<CancellationToken> action, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => action(token));
        }

        // なんだこれ。
        public static async Task WaitAsync(Task task, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await Task.Run(() =>
            {
                try
                {
                    task.Wait(token);
                }
                catch (OperationCanceledException)
                {
                }
            });
        }
    }

}
