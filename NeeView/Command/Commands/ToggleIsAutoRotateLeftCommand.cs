using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsAutoRotateLeftCommand : CommandElement
    {
        public ToggleIsAutoRotateLeftCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
#warning not implement yet
            //return new Binding(nameof(ContentCanvas.IsAutoRotateLeft)) { Source = MainViewComponent.Current.ContentCanvas };
            return new Binding("Dummy");
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return MainViewComponent.Current.ViewPropertyControl.GetAutoRotateLeft() ? Properties.Resources.ToggleIsAutoRotateLeftCommand_Off : Properties.Resources.ToggleIsAutoRotateLeftCommand_On;
        }
        
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewPropertyControl.SetAutoRotateLeft(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                MainViewComponent.Current.ViewPropertyControl.ToggleAutoRotateLeft();
            }
        }
    }
}
