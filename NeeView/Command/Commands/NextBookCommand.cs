namespace NeeView
{
    public class NextBookCommand : CommandElement
    {
        public NextBookCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.BookMove");
            this.ShortCutKey = "Down";
            this.MouseGesture = "LD";
            this.IsShowMessage = false;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            _ = BookshelfFolderList.Current.NextFolder();
        }
    }
}
