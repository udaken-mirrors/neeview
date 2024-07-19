namespace NeeView
{
    public class HelpSearchOptionCommand : CommandElement
    {
        public HelpSearchOptionCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            SearchOptionManual.OpenSearchOptionManual();
        }
    }
}
