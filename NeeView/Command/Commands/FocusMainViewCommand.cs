using System.Runtime.Serialization;

namespace NeeView
{
    public class FocusMainViewCommand : CommandElement
    {
        public FocusMainViewCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Panel;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new FocusMainViewCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainWindowModel.Current.FocusMainView(e.Parameter.Cast<FocusMainViewCommandParameter>(), e.Options.HasFlag(CommandOption.ByMenu));
        }
    }

}
