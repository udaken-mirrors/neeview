namespace NeeView
{
    public class ViewScaleStretchCommand : CommandElement
    {
        public ViewScaleStretchCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.ViewManipulation");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewTransformControl.Stretch(false);
        }
    }
}
