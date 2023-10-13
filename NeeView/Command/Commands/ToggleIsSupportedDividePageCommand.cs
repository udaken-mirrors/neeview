using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleIsSupportedDividePageCommand : CommandElement
    {
        public ToggleIsSupportedDividePageCommand()
        {
            this.Group = Properties.Resources.CommandGroup_PageSetting;
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return BindingGenerator.BindingBookSetting(nameof(BookSettings.Current.IsSupportedDividePage));
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return BookSettings.Current.IsSupportedDividePage ? Properties.Resources.ToggleIsSupportedDividePageCommand_Off : Properties.Resources.ToggleIsSupportedDividePageCommand_On;
        }

        public override bool CanExecute(object? sender, CommandContext e)
        {
            return BookSettings.Current.CanPageModeSubSetting(PageMode.SinglePage);
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                BookSettings.Current.SetIsSupportedDividePage(Convert.ToBoolean(e.Args[0]));
            }
            else
            {
                BookSettings.Current.ToggleIsSupportedDividePage();
            }
        }
    }
}
