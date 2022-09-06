using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{
    /// <summary>
    /// パイプを使って他のプロセスから送られてきたコマンドを受信する
    /// </summary>
    public class RemoteCommandServer : IDisposable
    {
        public static JsonSerializerOptions SerializerOptions { get; } = new JsonSerializerOptions();


        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();


        public RemoteCommandServer()
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLine(GetPipetName(process));
        }


        public EventHandler<RemoteCommandEventArgs>? Called;


        public void Start()
        {
            Task.Run(ReciverAsync);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        public static string GetPipetName(Process process)
        {
            return process.ProcessName + ".p" + process.Id;
        }

        private async Task ReciverAsync()
        {
            var pipeName = GetPipetName(Process.GetCurrentProcess());

            while (true)
            {
                try
                {
                    using var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In);

                    await pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);
                    if (pipeServer.IsConnected)
                    {
                        var command = await JsonSerializer.DeserializeAsync<RemoteCommand>(pipeServer, RemoteCommandServer.SerializerOptions, _cancellationTokenSource.Token);
                        if (command != null && Called != null)
                        {
                            ////Debug.WriteLine($"Recieve: {command.ID}({string.Join(",", command.Args)})");
                            Called(this, new RemoteCommandEventArgs(command));
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // 例外をここで潰してしまうのはどうなんだろう？
                    Debug.WriteLine($"Remote Server: {ex.Message}");
                }
            }

            Debug.WriteLine($"Remote Server: Stopped");
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }


    /// <summary>
    /// コマンド呼び出しイベントの引数
    /// </summary>
    public class RemoteCommandEventArgs : EventArgs
    {
        public RemoteCommandEventArgs(RemoteCommand command)
        {
            Command = command;
        }

        public RemoteCommand Command { get; set; }
    }
}
