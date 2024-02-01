namespace NeeView
{
    public class PrevBookCommand : CommandElement
    {
        public PrevBookCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.BookMove");
            this.ShortCutKey = "Up";
            this.MouseGesture = "LU";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            _ = BookshelfFolderList.Current.PrevFolder();
        }
    }
}
