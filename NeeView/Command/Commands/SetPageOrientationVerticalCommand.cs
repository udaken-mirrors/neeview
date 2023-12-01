using System.Windows.Data;

namespace NeeView
{
    public class SetPageOrientationVerticalCommand : CommandElement
    {
        public SetPageOrientationVerticalCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageFrameOrientation(PageFrameOrientation.Vertical);
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Properties.Resources.PageFrameOrientation_Vertical;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            Config.Current.Book.Orientation = PageFrameOrientation.Vertical;
        }
    }
}
