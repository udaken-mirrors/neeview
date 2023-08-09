namespace NeeView
{
    public class ViewScrollNTypeUpCommand : CommandElement
    {
        public ViewScrollNTypeUpCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ViewScrollNTypeCommandParameter());
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewController.ScrollNTypeUp(e.Parameter.Cast<ViewScrollNTypeCommandParameter>());
        }
    }

}
