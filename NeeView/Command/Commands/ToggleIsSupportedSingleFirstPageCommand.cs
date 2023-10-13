using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedSingleFirstPageCommand : CommandElement
    {
        public ToggleIsSupportedSingleFirstPageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(Config.Current.BookSetting.IsSupportedSingleFirstPage));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.BookSetting.IsSupportedSingleFirstPage ? Properties.Resources.ToggleIsSupportedSingleFirstPageCommand_Off : Properties.Resources.ToggleIsSupportedSingleFirstPageCommand_On;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanPageModeSubSetting(PageMode.WidePage);
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.BookSetting.IsSupportedSingleFirstPage = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.BookSetting.IsSupportedSingleFirstPage = !Config.Current.BookSetting.IsSupportedSingleFirstPage;
            }
        }
    }
}
