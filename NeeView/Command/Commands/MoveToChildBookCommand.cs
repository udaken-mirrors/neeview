namespace NeeView
{
    public class MoveToChildBookCommand : CommandElement
    {
        public MoveToChildBookCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.BookMove");
            this.ShortCutKey = "Alt+Down";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.CanMoveToChildBook();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.MoveToChildBook(this);
        }
    }
}
