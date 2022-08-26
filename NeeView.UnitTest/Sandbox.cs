using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
