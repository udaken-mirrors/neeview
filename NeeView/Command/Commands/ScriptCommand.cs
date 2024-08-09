using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class ScriptCommand : CommandElement
    {
        private class ScriptCommandParameterDecorator : ICommandParameterDecorator
        {
            private readonly string _path;
            private readonly ScriptCommandSourceMap _sourceMap;

            public ScriptCommandParameterDecorator(string path, ScriptCommandSourceMap sourceMap)
            {
                _path = path;
                _sourceMap = sourceMap;
            }

            public void DecorateCommandParameter(CommandParameter parameter)
            {
                if (parameter is not ScriptCommandParameter scriptCommandParameter) return;

                if (_sourceMap.TryGetValue(_path, out var source))
                {
                    scriptCommandParameter.Argument = source.Args;
                }
            }
        }


        public const string Prefix = "Script_";
        public const string EventOnBookLoaded = Prefix + ScriptCommandSource.OnBookLoadedFilename;
        public const string EventOnPageChanged = Prefix + ScriptCommandSource.OnPageChangedFilename;

        private readonly string _path;
        private readonly ScriptCommandSourceMap _sourceMap;
        private GesturesMemento? _defaultGestures;
        private string? _defaultArgs;

        public ScriptCommand(string path, ScriptCommandSourceMap sourceMap) : base(PathToScriptCommandName(path))
        {
            _path = path;
            _sourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));

            this.Group = Properties.TextResources.GetString("CommandGroup.Script");
            this.Text = LoosePath.GetFileNameWithoutExtension(_path);

            this.ParameterSource = new CommandParameterSource(new ScriptCommandParameter(), new ScriptCommandParameterDecorator(path, sourceMap));

            UpdateDocument(true);
        }


        public string Path => _path;


        public static bool IsScriptCommandName(string name)
        {
            return name.StartsWith(Prefix);
        }

        public static string PathToScriptCommandName(string path)
        {
            return Prefix + LoosePath.GetFileNameWithoutExtension(path);
        }

        protected override CommandElement CloneInstance()
        {
            var command = new ScriptCommand(_path, _sourceMap);
            if (_sourceMap.TryGetValue(_path, out var source))
            {
                command.Text = source.Text;
                command.Remarks = source.Remarks;
                command.ShortCutKey = ShortcutKey.Empty;
                command.TouchGesture = TouchGesture.Empty;
                command.MouseGesture = MouseSequence.Empty;
            }
            return command;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ScriptManager.Current.Execute(sender, _path, Name, (e.Parameter.Cast<ScriptCommandParameter>()).Argument);
        }

        public override void UpdateDefaultParameter()
        {
            StoreDefaultArgs();
        }

        private void StoreDefaultGesture()
        {
            _defaultGestures = CreateGesturesMemento();
        }

        private void StoreDefaultArgs()
        {
            _defaultArgs = GetDefaultArgs();
        }

        private string? GetDefaultArgs()
        {
            return (ParameterSource?.GetDefault() as ScriptCommandParameter)?.Argument;
        }

        private ScriptCommandParameter GetScriptCommandParameter()
        {
            return (Parameter as ScriptCommandParameter) ?? throw new InvalidOperationException();
        }

        public void UpdateDocument(bool isForce)
        {
            if (_sourceMap.TryGetValue(_path, out var source))
            {
                IsCloneable = source.IsCloneable;

                Text = source.Text;
                if (IsCloneCommand())
                {
                    Text += " " + NameSource.Number.ToString();
                }

                Remarks = source.Remarks;

                if (isForce || (_defaultGestures != null && _defaultGestures.IsEquals(this) && !IsCloneCommand()))
                {
                    ShortCutKey = new ShortcutKey(source.ShortCutKey);
                    MouseGesture = new MouseSequence(source.MouseGesture);
                    TouchGesture = new TouchGesture(source.TouchGesture);
                    StoreDefaultGesture();
                }

                var parameter = GetScriptCommandParameter();
                if (isForce || parameter.Argument == _defaultArgs)
                {
                    parameter.Argument = source.Args;
                    StoreDefaultArgs();
                }
            }
        }

        public void OpenFile()
        {
            ExternalProcess.OpenWithTextEditor(_path);
        }
    }

}
