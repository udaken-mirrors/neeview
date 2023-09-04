using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Xunit.Abstractions;

namespace NeeView.UnitTest
{
    public class Sandbox
    {
        private readonly ITestOutputHelper _output;

        public Sandbox(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }

        [Fact(Timeout = 2000)]
        public async void EventFlagTest()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var job = Job.Create(null!, cancellationTokenSource.Token);

            _output.WriteLine("start...");
            var sw = Stopwatch.StartNew();
            _ = Task.Run(() =>
            {
                _output.WriteLine("task start");
                Task.Delay(1000).Wait();
                _output.WriteLine("job disposed.");
                job.Dispose();
            });

            _output.WriteLine("wait...");
            await job.WaitAsync(-1, CancellationToken.None);
            sw.Stop();
            _output.WriteLine($"done. {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void DispatcherTest()
        {
            Dispatcher? myDispatcher = null;

            var dispatcherReadyEvent = new ManualResetEvent(false);

            new Thread(new ThreadStart(() =>
            {
                myDispatcher = Dispatcher.CurrentDispatcher;
                dispatcherReadyEvent.Set();
                Dispatcher.Run();
            })).Start();

            dispatcherReadyEvent.WaitOne();
            if (myDispatcher is null) throw new InvalidOperationException();

            myDispatcher.Invoke(() => _output.WriteLine("invoke 1"));
            myDispatcher.InvokeShutdown();

            Assert.Throws<TaskCanceledException>(() =>
                myDispatcher.Invoke(() => _output.WriteLine("invoke 2")));

            _output.WriteLine("done.");
        }

        [Fact]
        public void RectTest()
        {
            var rect = new Rect(10, 20, 30, 40);
            Assert.Equal(Rect.Inflate(rect, -1, +1), rect.InflateValid(-1, +1));
            Assert.Equal(Rect.Inflate(rect, +2, -2), rect.InflateValid(+2, -2));

            var result1 = rect.InflateValid(-20, -10);
            Assert.Equal(0.0, result1.Width);
            Assert.Equal(20.0, result1.Height);
            Assert.Equal(25.0, result1.X);
            Assert.Equal(30.0, result1.Y);
        }
    }
}
