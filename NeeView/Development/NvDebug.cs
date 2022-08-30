using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public static class NvDebug
    {
        [Conditional("DEBUG")]
        public static void __DumpThread(string? s = null)
        {
            Debug.WriteLine($"> ThreadId: {Thread.CurrentThread.ManagedThreadId}: {s}");
        }

        [Conditional("DEBUG")]
        private static void __Delay(int ms)
        {
            Thread.Sleep(ms);
        }

        public static void MeasureAction(Action action)
        {
            var callStack = new StackFrame(1, true);
            var sourceFile = System.IO.Path.GetFileName(callStack.GetFileName());
            int sourceLine = callStack.GetFileLineNumber();
            var sw = Stopwatch.StartNew();

            action.Invoke();

            Debug.WriteLine($"AppDispatcher.Invoke: {sourceFile}({sourceLine}):  {sw.ElapsedMilliseconds}ms");
        }

        public static TResult MeasureFunc<TResult>(Func<TResult> func)
        {
            var callStack = new StackFrame(1, true);
            var sourceFile = System.IO.Path.GetFileName(callStack.GetFileName());
            int sourceLine = callStack.GetFileLineNumber();
            var sw = Stopwatch.StartNew();

            var result = func.Invoke();

            Debug.WriteLine($"AppDispatcher.Invoke: {sourceFile}({sourceLine}):  {sw.ElapsedMilliseconds}ms");

            return result;
        }
    }
}
