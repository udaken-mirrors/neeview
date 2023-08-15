﻿using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsLoupeCommand : CommandElement
    {
        public ToggleIsLoupeCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }
        public override Binding CreateIsCheckedBinding()
        {
#warning not implement yet
            //return new Binding(nameof(LoupeTransform.IsEnabled)) { Mode = BindingMode.OneWay, Source = MainViewComponent.Current.LoupeTransform };
            return new Binding("Dummy");
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewLoupeControl.GetLoupeMode() ? Properties.Resources.ToggleIsLoupeCommand_Off : Properties.Resources.ToggleIsLoupeCommand_On;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewLoupeControl.SetLoupeMode(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                MainViewComponent.Current.ViewLoupeControl.ToggleLoupeMode();
            }
        }
    }
}
