using System.Runtime.Serialization;

namespace NeeView
{
    public class OpenExternalAppCommand : CommandElement
    {
        public OpenExternalAppCommand()
        {
            this.Group = Properties.Resources.CommandGroup_File;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new OpenExternalAppCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookOperation.Current.Control.CanOpenFilePlace();
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookOperation.Current.Control.OpenApplication(e.Parameter.Cast<OpenExternalAppCommandParameter>());
        }
    }

}
