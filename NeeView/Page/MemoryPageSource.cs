using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using NeeLaboratory.Threading;
using NeeView.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// FileCache maybe.
    /// </summary>
    public class MemoryPageSource : PageSource<byte[]>
    {
        public MemoryPageSource(ArchiveEntry entry) : base(entry)
        {
        }


        public override long DataSize => Data?.LongLength ?? 0;


        /// <summary>
        /// 読み込み。キャンセル等された場合でも正常終了する。
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected override async Task LoadAsyncCore(CancellationToken token)
        {
            // TODO: Susie

            try
            {
                //Debug.WriteLine($"Loading...: {ArchiveEntry}");
#if DEBUG
                if (Debugger.IsAttached)
                {
                    NVDebug.AssertMTA();
                    await Task.Delay(200, token);
                }
#endif
                NVDebug.AssertMTA();

                // memory chache
                using var stream = ArchiveEntry.OpenEntry();
                var length = stream.Length;
                var buffer = new byte[length];
                var readSize = await stream.ReadAsync(buffer, 0, (int)length, token);
                if (readSize < length) throw new IOException("This file size is too large to read.");

                //Debug.WriteLine($"Loaded: {ArchiveEntry}, {token.IsCancellationRequested}");
                SetData(buffer, null);
            }
            catch (OperationCanceledException)
            {
                //Debug.WriteLine($"Load Canceled: {ArchiveEntry}");
                SetData(null, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                SetData(null, ex.Message);
                throw;
            }
        }


        protected override void UnloadCore()
        {
            if (!IsFailed)
            {
                SetData(null, null);
            }
        }
    }
}
