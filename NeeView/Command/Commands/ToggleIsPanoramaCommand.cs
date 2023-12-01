using System.Windows.Data;

namespace NeeView
{
    public class ToggleIsPanoramaCommand : CommandElement
    {
        public ToggleIsPanoramaCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(BookConfig.IsPanorama)) { Source = Config.Current.Book };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.Book.IsPanorama ? Properties.Resources.ToggleIsPanoramaCommand_Off : Properties.Resources.ToggleIsPanoramaCommand_On;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Book.IsPanorama = !Config.Current.Book.IsPanorama;
        }
    }
}
