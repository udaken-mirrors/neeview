namespace NeeView
{
    public class FocusPrevAppCommand : CommandElement
    {
        public FocusPrevAppCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Window");
            this.ShortCutKey = "Ctrl+Shift+Tab";
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            WindowActivator.NextActivate(-1);
        }
    }

}
