using NeeLaboratory.ComponentModel;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ToggleStretchModeCommand : CommandElement
    {
        public ToggleStretchModeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ImageScale;
            this.ShortCutKey = "LeftButton+WheelDown";
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new ToggleStretchModeCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewController.GetToggleStretchMode(e.Parameter.Cast<ToggleStretchModeCommandParameter>()).ToAliasName();
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.View.StretchMode = MainViewComponent.Current.ViewController.GetToggleStretchMode(e.Parameter.Cast<ToggleStretchModeCommandParameter>());
        }
    }

}
