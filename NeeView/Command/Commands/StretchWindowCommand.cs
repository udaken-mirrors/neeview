namespace NeeView
{
    public class StretchWindowCommand : CommandElement
    {
        public StretchWindowCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Window");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.StretchWindow();
        }
    }
}
