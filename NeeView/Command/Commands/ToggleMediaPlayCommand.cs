namespace NeeView
{
    public class ToggleMediaPlayCommand : CommandElement
    {
        public ToggleMediaPlayCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Video");
        }
        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookOperation.Current.IsMediaPlaying() ? Properties.TextResources.GetString("Word.Stop") : Properties.TextResources.GetString("Word.Play");
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.MediaExists();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.ToggleMediaPlay();
        }
    }
}
