using System.Windows.Data;


namespace NeeView
{
    public class SetPageModeTwoCommand : CommandElement
    {
        public SetPageModeTwoCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.PageSetting");
            this.ShortCutKey = "Ctrl+2";
            this.MouseGesture = "RD";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.WidePage);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanEdit;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettings.Current.SetPageMode(PageMode.WidePage);
        }
    }
}
