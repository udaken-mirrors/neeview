using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleViewFlipHorizontalCommand : CommandElement
    {
        public ToggleViewFlipHorizontalCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
#warning not support yet
            //return new Binding(nameof(DragTransform.IsFlipHorizontal)) { Source = MainViewComponent.Current.DragTransform, Mode = BindingMode.OneWay };
            return new Binding("Dummy");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewTransformControl.FlipHorizontal(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                MainViewComponent.Current.ViewTransformControl.ToggleFlipHorizontal();
            }
        }
    }
}
