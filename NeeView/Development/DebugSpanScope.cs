using System;
using System.Diagnostics;

namespace NeeView
{
    public class DebugSpanScope : IDisposable
    {
        private readonly Stopwatch _sw;
        private readonly string _label;
        private readonly long _start;

        public DebugSpanScope(Stopwatch sw, string label)
        {
            Debug.Assert(sw.IsRunning);
            _sw = sw;
            _label = label;
            _start = sw.ElapsedMilliseconds;
        }

        public void Dump()
        {
            var now = _sw.ElapsedMilliseconds;
            Debug.WriteLine($"{_label}: {now}ms ({now - _start}ms)");
        }

        public static void Dump(Stopwatch sw, string label)
        {
            var now = sw.ElapsedMilliseconds;
            Debug.WriteLine($"{label}: {now}ms");
        }

        public void Dispose()
        {
            Dump();
        }
    }

}
