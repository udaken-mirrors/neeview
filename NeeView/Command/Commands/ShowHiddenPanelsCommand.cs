namespace NeeView
{
    public class ShowHiddenPanelsCommand : CommandElement
    {
        public ShowHiddenPanelsCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Window");
            this.TouchGesture = new TouchGesture("TouchCenter");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.EnterVisibleLocked();
        }
    }
}
