using System;
using System.Windows.Data;


namespace NeeView
{
    public class TogglePermitFileCommand : CommandElement
    {
        public TogglePermitFileCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Other");
            this.IsShowMessage = true;
        }

        public override Binding CreateIsCheckedBinding()
        {
            return new Binding(nameof(SystemConfig.IsFileWriteAccessEnabled)) { Source = Config.Current.System, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            return Config.Current.System.IsFileWriteAccessEnabled ? Properties.TextResources.GetString("TogglePermitFileCommand.Off") : Properties.TextResources.GetString("TogglePermitFileCommand.On");
        }

        [MethodArgument("@ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                Config.Current.System.IsFileWriteAccessEnabled = Convert.ToBoolean(e.Args[0]);
            }
            else
            {
                Config.Current.System.IsFileWriteAccessEnabled = !Config.Current.System.IsFileWriteAccessEnabled;
            }
        }
    }
}
