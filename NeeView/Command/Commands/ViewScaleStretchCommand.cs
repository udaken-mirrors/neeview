namespace NeeView
{
    public class ViewScaleStretchCommand : CommandElement
    {
        public ViewScaleStretchCommand()
        {
            this.Group = Properties.Resources.CommandGroup_ViewManipulation;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.Stretch(false);
        }
    }
}
