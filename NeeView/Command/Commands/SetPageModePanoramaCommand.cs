using System.Windows.Data;


namespace NeeView
{
    public class SetPageModePanoramaCommand : CommandElement
    {
        public SetPageModePanoramaCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.ShortCutKey = "Ctrl+3";
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.PageMode(PageMode.Panorama);
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            BookSettingPresenter.Current.SetPageMode(PageMode.Panorama);
        }
    }
}
