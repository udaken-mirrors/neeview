using NeeView.Windows.Property;

namespace NeeView
{
    public class NextPlaylistItemInBookCommand : CommandElement
    {
        public NextPlaylistItemInBookCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Playlist");
            this.IsShowMessage = false;

            // PrevPlaylistItemInBook
            this.ParameterSource = new CommandParameterSource(new MovePlaylsitItemInBookCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Playlist.CanNextMarkInPlace(e.Parameter.Cast<MovePlaylsitItemInBookCommandParameter>());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Playlist.NextMarkInPlace(sender, e.Parameter.Cast<MovePlaylsitItemInBookCommandParameter>());
        }
    }

}
