namespace NeeView
{
    public class ToggleBookReadOrderCommand : CommandElement
    {
        public ToggleBookReadOrderCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.BookReadOrder.GetToggle().ToAliasName();
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.ToggleBookReadOrder();
        }
    }
}
