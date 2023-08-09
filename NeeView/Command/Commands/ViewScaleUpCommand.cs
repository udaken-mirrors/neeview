using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ViewScaleUpCommand : CommandElement
    {
        public ViewScaleUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.ShortCutKey = "RightButton+WheelUp";
            this.IsShowMessage = false;
            this.ParameterSource = new CommandParameterSource(new ViewScaleCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScaleUp(e.Parameter.Cast<ViewScaleCommandParameter>());
        }
    }
}
