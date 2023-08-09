namespace NeeView
{
    public class ExportBackupCommand : CommandElement
    {
        public ExportBackupCommand()
        {
            this.Group = Properties.Resources.CommandGroup_Other;
            this.IsShowMessage = false;

            this.ParameterSource = new CommandParameterSource(new ExportBackupCommandParameter());
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return !NowLoading.Current.IsDispNowLoading;
        }

        public override void Execute(object? sender, CommandContext e)
        {
            ExportDataPresenter.Current.Export(e.Parameter.Cast<ExportBackupCommandParameter>());
        }
    }
}
