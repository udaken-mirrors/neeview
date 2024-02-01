namespace NeeView
{
    public class PrevPlaylistItemInBookCommand : CommandElement
    {
        public PrevPlaylistItemInBookCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Playlist");
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new MovePlaylsitItemInBookCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Playlist.CanPrevMarkInPlace(e.Parameter.Cast<MovePlaylsitItemInBookCommandParameter>());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Playlist.PrevMarkInPlace(sender, e.Parameter.Cast<MovePlaylsitItemInBookCommandParameter>());
        }
    }
}
