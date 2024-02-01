namespace NeeView
{
    public class LoupeOnCommand : CommandElement
    {
        public LoupeOnCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewLoupeControl.SetLoupeMode(true);
        }
    }
}
