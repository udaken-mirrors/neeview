﻿namespace NeeView
{
    public class TogglePageModeReverseCommand : CommandElement
    {
        public const string DefaultMouseGesture = "RU";

        public TogglePageModeReverseCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.MouseGesture = DefaultMouseGesture;
            this.IsShowMessage = true;

            this.ParameterSource = new CommandParameterSource(new TogglePageModeCommandParameter());
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettingPresenter.Current.LatestSetting.PageMode.GetToggle(-1, e.Parameter.Cast<TogglePageModeCommandParameter>().IsLoop).ToAliasName();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettingPresenter.Current.TogglePageMode(-1, e.Parameter.Cast<TogglePageModeCommandParameter>().IsLoop);
        }
    }
}