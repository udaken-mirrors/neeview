namespace NeeView
{
    public class HelpMainMenuCommand : CommandElement
    {
        public HelpMainMenuCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainMenuManual.OpenMainMenuManual();
        }
    }
}
