using NeeLaboratory.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class ViewScrollUpCommand : CommandElement
    {
        public ViewScrollUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollCommandParameter());
        }
        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.ScrollUp(e.Parameter.Cast<ViewScrollCommandParameter>());
        }
    }

}
