namespace NeeView
{
    public class CancelScriptCommand : CommandElement
    {
        public CancelScriptCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Script");
            this.IsShowMessage = false;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ScriptManager.Current.CancelAll();
        }
    }
}
