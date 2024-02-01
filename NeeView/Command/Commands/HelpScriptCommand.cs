namespace NeeView
{
    public class HelpScriptCommand : CommandElement
    {
        public HelpScriptCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            new ScriptManual().OpenScriptManual();
        }
    }
}
