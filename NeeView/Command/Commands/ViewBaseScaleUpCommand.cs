﻿namespace NeeView
{
    public class ViewBaseScaleUpCommand : CommandElement
    {
        public ViewBaseScaleUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScaleUp(ScaleType.BaseScale, e.Parameter.Cast<ViewScaleCommandParameter>());
        }
    }

}