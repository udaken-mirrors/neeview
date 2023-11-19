namespace NeeView
{
    public class FocusHistorySearchBoxCommand : CommandElement
    {
        public FocusHistorySearchBoxCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            SidePanelFrame.Current.FocusHistorySearchBox(e.Options.HasFlag(CommandOption.ByMenu));
        }
    }
}
