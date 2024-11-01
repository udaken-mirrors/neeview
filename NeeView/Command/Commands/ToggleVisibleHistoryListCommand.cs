using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleHistoryListCommand : CommandElement
    {
        public ToggleVisibleHistoryListCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("H");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleHistoryList)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleHistoryList ? Properties.TextResources.GetString("ToggleVisibleHistoryListCommand.Off") : Properties.TextResources.GetString("ToggleVisibleHistoryListCommand.On");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleHistoryList(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture), true, true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleHistoryList(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
