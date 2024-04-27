using System;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleVisibleEffectInfoCommand : CommandElement
    {
        public ToggleVisibleEffectInfoCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Panel");
            this.ShortCutKey = new ShortcutKey("E");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SidePanelFrame.IsVisibleEffectInfo)) { Source = SidePanelFrame.Current };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return SidePanelFrame.Current.IsVisibleEffectInfo ? Properties.TextResources.GetString("ToggleVisibleEffectInfoCommand.Off") : Properties.TextResources.GetString("ToggleVisibleEffectInfoCommand.On");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                SidePanelFrame.Current.SetVisibleEffectInfo(Convert.ToBoolean(e.Args[0]), true);
            }
            else
            {
                SidePanelFrame.Current.ToggleVisibleEffectInfo(e.Options.HasFlag(CommandOption.ByMenu));
            }
        }
    }
}
