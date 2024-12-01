﻿using NeeLaboratory.Linq;
using NeeView.Data;
using NeeView.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public enum SwitchOption
    {
        off,
        on,
    }

    public enum WindowStateOption
    {
        normal,
        min,
        max,
        full
    }

    public static class WindowStateOptionExtensions
    {
        public static WindowStateEx ToWindowStateEx(this WindowStateOption? option)
        {
            return option switch
            {
                WindowStateOption.normal => WindowStateEx.Normal,
                WindowStateOption.min => WindowStateEx.Minimized,
                WindowStateOption.max => WindowStateEx.Maximized,
                WindowStateOption.full => WindowStateEx.FullScreen,
                _ => WindowStateEx.None,
            };
        }

    }

    public class CommandLineOption
    {
        [OptionMember("h", "help", Default = "true", HelpText = "@AppOption.IsHelp")]
        public bool IsHelp { get; set; }

        [OptionMember("v", "version", Default = "true", HelpText = "@AppOption.IsVersion")]
        public bool IsVersion { get; set; }

        [OptionMember("x", "setting", HasParameter = true, RequireParameter = true, HelpText = "@AppOption.SettingFilename")]
        public string? SettingFilename { get; set; }

        [OptionMember("b", "blank", Default = "on", HelpText = "@AppOption.IsBlank")]
        public SwitchOption IsBlank { get; set; }

        [OptionMember("r", "reset-placement", Default = "on", HelpText = "@AppOption.IsResetPlacement")]
        public SwitchOption IsResetPlacement { get; set; }

        [OptionMember("n", "new-window", Default = "on", HasParameter = true, HelpText = "@AppOption.IsNewWindow")]
        public SwitchOption? IsNewWindow { get; set; }

        [OptionMember("s", "slideshow", Default = "on", HasParameter = true, HelpText = "@AppOption.IsSlideShow")]
        public SwitchOption? IsSlideShow { get; set; }

        [OptionMember("o", "folderlist", HasParameter = true, RequireParameter = true, HelpText = "@AppOption.FolderList")]
        public string? FolderList { get; set; }

        [OptionMember(null, "window", HasParameter = true, RequireParameter = true, HelpText = "@AppOption.WindowState")]
        public WindowStateOption? WindowState { get; set; }

        [OptionMember(null, "script", HasParameter = true, RequireParameter = true, HelpText = "@AppOption.ScriptFile")]
        public string? ScriptFile { get; set; }

        [OptionMember(null, "clear-registry", Default = "true", HelpText = "@AppOption.ClearRegistry")]
        public bool IsClearRegistry { get; set; }

        [OptionValues]
        public List<string> Values { get; set; } = new List<string>();


        public QueryPath? FolderListQuery { get; private set; }
        public QueryPath? ScriptQuery { get; private set; }


        public void Validate()
        {
            try
            {
                Values = Values.Select(e => Path.GetFullPath(e)).WhereNotNull().ToList();

                FolderListQuery = GetFullQueryPath(FolderList);
                ScriptQuery = GetFullQueryPath(ScriptFile);

                if (this.SettingFilename != null)
                {
                    if (!File.Exists(this.SettingFilename))
                    {
                        throw new ArgumentException($"{Properties.TextResources.GetString("OptionArgumentException.FileNotFound")}: {this.SettingFilename}");
                    }
                    this.SettingFilename = Path.GetFullPath(this.SettingFilename);
                }
                else
                {
                    this.SettingFilename = Path.Combine(Environment.LocalApplicationDataPath, SaveDataProfile.UserSettingFileName);
                }
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, Properties.TextResources.GetString("BootErrorDialog.Title")).ShowDialog();
                throw new OperationCanceledException("Wrong startup parameter");
            }
        }


        private QueryPath? GetFullQueryPath(string? src)
        {
            if (string.IsNullOrWhiteSpace(src)) return null;

            var query = new QueryPath(src);
            if (query.Path is null) return null;
            
            if (query.Scheme != QueryScheme.File)
            {
                return query;
            }

            return query.ReplacePath(Path.GetFullPath(query.Path));
        }
    }


    public partial class App
    {
        public static CommandLineOption ParseArguments(string[] args)
        {
            var optionMap = new OptionMap<CommandLineOption>();
            CommandLineOption option;

            try
            {
                var items = new List<string>(args);

                // プロトコル起動を吸収
                const string scheme = "neeview-open:";
                if (items.Any() && items[0].StartsWith(scheme, StringComparison.Ordinal))
                {
                    items[0] = items[0].Replace(scheme, "", StringComparison.Ordinal);
                    if (string.IsNullOrWhiteSpace(items[0]))
                    {
                        items.RemoveAt(0);
                    }
                }

                option = optionMap.ParseArguments(items.ToArray());
            }
            catch (Exception ex)
            {
                new MessageDialog(ex.Message, NeeView.Properties.TextResources.GetString("BootErrorDialog.Title")).ShowDialog();
                throw new OperationCanceledException("Wrong startup parameter");
            }

            if (option.IsHelp)
            {
                var dialog = new MessageDialog(optionMap.GetCommandLineHelpText(), NeeView.Properties.TextResources.GetString("BootOptionDialog.Title"));
                dialog.SizeToContent = SizeToContent.WidthAndHeight;
                dialog.ContentRendered += (s, e) => dialog.InvalidateVisual();
                dialog.ShowDialog();
                throw new OperationCanceledException("Show command line help");
            }

            if (option.IsClearRegistry)
            {
                Environment.ClearRegistry();
                throw new OperationCanceledException("Clear registries for shell");
            }

            return option;
        }
    }
}
