using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class ScriptCommand : CommandElement
    {
        public const string Prefix = "Script_";
        public const string EventOnBookLoaded = Prefix + ScriptCommandSource.OnBookLoadedFilename;
        public const string EventOnPageChanged = Prefix + ScriptCommandSource.OnPageChangedFilename;

        private readonly string _path;
        private readonly ScriptCommandSourceMap _sourceMap;
        private GesturesMemento? _defaultGestures;

        public ScriptCommand(string path, ScriptCommandSourceMap sourceMap) : base(PathToScriptCommandName(path))
        {
            _path = path;
            _sourceMap = sourceMap ?? throw new ArgumentNullException(nameof(sourceMap));

            this.Group = Properties.TextResources.GetString("CommandGroup.Script");
            this.Text = LoosePath.GetFileNameWithoutExtension(_path);

            this.ParameterSource = new CommandParameterSource(new ScriptCommandParameter());

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

        private void StoreDefault()
        {
            _defaultGestures = CreateGesturesMemento();
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

                    StoreDefault();
                }
            }
        }

        public void OpenFile()
        {
            ExternalProcess.OpenWithTextEditor(_path);
        }
    }

}
