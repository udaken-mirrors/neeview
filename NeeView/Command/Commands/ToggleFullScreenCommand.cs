using System.Windows.Data;


namespace NeeView
{
    public class ToggleFullScreenCommand : CommandElement
    {
        public ToggleFullScreenCommand()
        {
            this.Group = Properties.TextResources.GetString("CommandGroup.Window");
            this.ShortCutKey = new ShortcutKey("F11");
            this.MouseGesture = new MouseSequence("U");
            this.IsShowMessage = false;
        }

        public override Binding CreateIsCheckedBinding()
        {
            var windowStateManager =  MainWindow.Current.WindowStateManager;
            return new Binding(nameof(windowStateManager.IsFullScreen)) { Source = windowStateManager, Mode = BindingMode.OneWay };
        }

        public override string ExecuteMessage(object? sender, CommandContext e)
        {
            var windowStateManager = MainWindow.Current.WindowStateManager;
            return windowStateManager.IsFullScreen ? Properties.TextResources.GetString("ToggleFullScreenCommand.Off") : Properties.TextResources.GetString("ToggleFullScreenCommand.On");
        }

        public override void Execute(object? sender, CommandContext e)
        {
            MainViewComponent.Current.ViewWindowControl.ToggleWindowFullScreen(sender);
        }
    }
}
