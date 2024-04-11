using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Jint;

namespace NeeView
{
    public class JavaScriptEngine
    {
        private readonly Jint.Engine _engine;
        private readonly CommandHost _commandHost;
        private CancellationToken _cancellationToken;


        public JavaScriptEngine()
        {
            _commandHost = new CommandHost();
            _engine = new Jint.Engine(config => config
                .DebugMode(true)
                .AllowClr(typeof(System.Diagnostics.Process).Assembly));

            _engine.SetValue("sleep", (Action<int>)Sleep);
            _engine.SetValue("log", (Action<object>)Log);
            _engine.SetValue("system", (Action<string, string>)SystemCall);
            _engine.SetValue("include", (Func<string, object?>)ExecuteFile);
            _engine.SetValue("nv", _commandHost);
        }


        public string? CurrentPath { get; private set; }

        public string? CurrentFolder { get; set; }

        public bool IsToastEnable { get; set; }

        public bool IsDirty => _commandHost.IsDirty;


        [Documentable(Name = "nv")]
        public CommandHost CommandHost => _commandHost;


        public void SetArgs(List<string> args)
        {
            _commandHost.SetArgs(args);
        }

        [Documentable(Name = "include")]
        public object? ExecuteFile(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));

            return ExecuteFile(path, _cancellationToken);
        }

        public object? ExecuteFile(string path, CancellationToken token)
        {
            var fullPath = GetFullPath(path);
            string script = File.ReadAllText(fullPath, Encoding.UTF8);

            var oldFolder = CurrentFolder;
            try
            {
                CurrentFolder = LoosePath.GetDirectoryName(fullPath);
                return Execute(fullPath, script, token);
            }
            finally
            {
                CurrentFolder = oldFolder;
            }
        }

        public object? Execute(string? path, string script, CancellationToken token)
        {
            _cancellationToken = token;
            _commandHost.SetCancellationToken(token);

            var oldPath = CurrentPath;
            try
            {
                CurrentPath = path;
                var result = path is null ? _engine.Evaluate(script) : _engine.Evaluate(script, path);
                return result?.ToObject();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ScriptException)
            {
                throw;
            }
            catch (Esprima.ParserException ex) when (ex.Error is not null)
            {
                throw new ScriptException(new ScriptNotice(ex.Error), ex);
            }
            catch (Exception ex)
            {
                throw new ScriptException(CreateScriptErrorMessage(ex.Message), ex);
            }
            finally
            {
                CurrentPath = oldPath;
            }
        }

        public void ExceptionProcess(Exception ex)
        {
            var message = ex switch
            {
                Esprima.ParserException pex when pex.Error is not null
                    => new ScriptNotice(pex.Error).ToString(),
                OperationCanceledException or ScriptException
                    => ex.Message,
                _
                    => CreateScriptErrorMessage(ex.Message).ToString(),
            };

            ConsoleWindowManager.Current.ErrorMessage(message, this.IsToastEnable);
        }

        [Documentable(Name = "log")]
        public void Log(object log)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var message = log as string ?? new JsonStringBulder(log).ToString();
            ConsoleWindowManager.Current.WriteLine(ScriptMessageLevel.None, message);
        }

        [Documentable(Name = "sleep")]
        public void Sleep(int millisecond)
        {
            // https://qiita.com/laughter/items/b0bcab9c60d0a28709a0
            if (_cancellationToken.WaitHandle.WaitOne(millisecond))
            {
                throw new OperationCanceledException();
            }
        }

        [Documentable(Name = "system")]
        public void SystemCall(string filename, string? args = null)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            ExternalProcess.Start(filename, args, new ExternalProcessOptions() { IsThrowException = true });
        }


        public void SetValue(string name, object value)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            _engine.SetValue(name, value);
        }

        public object GetValue(string name)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            return _engine.GetValue(name).ToObject();
        }

        private string GetFullPath(string path)
        {
            if (CurrentFolder != null && !Path.IsPathRooted(path))
            {
                path = Path.Combine(CurrentFolder, path);
            }

            return Path.GetFullPath(path);
        }

        public ScriptNotice CreateScriptErrorMessage(Exception ex)
        {
            return ex switch
            {
                Esprima.ParserException pex when pex.Error is not null
                    => new ScriptNotice(pex.Error),
                _
                    => CreateScriptErrorMessage(ex.Message),
            };
        }

        public ScriptNotice CreateScriptErrorMessage(string s)
        {
            var location = _engine.Debugger?.CurrentLocation;

            string? source = null;
            int line = -1;
            string message = s.Trim();

            var regex = new Regex(@"^Line\s*(\d+):(.+)$", RegexOptions.IgnoreCase);
            var match = regex.Match(s);
            if (match.Success)
            {
                line = int.Parse(match.Groups[1].Value);
                message = match.Groups[2].Value.Trim();
            }
            if (location.HasValue)
            {
                source = location.Value.Source;
                line = location.Value.Start.Line;
            }

            return new ScriptNotice(source, line, message);
        }



        internal WordNode CreateWordNode(string name)
        {
            return _commandHost.CreateWordNode(name);
        }
    }

}
