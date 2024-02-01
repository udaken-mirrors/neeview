namespace NeeView
{
    public class FocusBookmarkSearchBoxCommand : CommandElement
    {
        public FocusBookmarkSearchBoxCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Panel");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            SidePanelFrame.Current.FocusBookmarkSearchBox(e.Options.HasFlag(CommandOption.ByMenu));
        }
    }
}
