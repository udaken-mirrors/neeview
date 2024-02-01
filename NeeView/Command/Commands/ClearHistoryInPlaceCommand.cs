namespace NeeView
{
    public class ClearHistoryInPlaceCommand : CommandElement
    {
        public ClearHistoryInPlaceCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.File");
            this.IsShowMessage = true;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookshelfFolderList.Current.Place != null;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookshelfFolderList.Current.ClearHistory();
        }
    }
}
