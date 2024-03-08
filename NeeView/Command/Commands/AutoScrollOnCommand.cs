using System;
using System.Windows.Data;


namespace NeeView
{
    public class AutoScrollOnCommand : CommandElement
    {
        public AutoScrollOnCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ViewManipulation");
            this.ShortCutKey = "MiddleClick";
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewAutoScrollControl.SetAutoScrollMode(true);
        }
    }
}
