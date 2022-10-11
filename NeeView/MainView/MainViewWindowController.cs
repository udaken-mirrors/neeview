using NeeView.Windows;
using System.Windows;

namespace NeeView
{
    public class MainViewWindowController : WindowController
    {
        public MainViewWindowController(Window window, WindowStateManager windowStateManager) : base(window, windowStateManager)
        {
        }

        public override bool IsTopmost
        {
            get { return Config.Current.MainView.IsTopmost; }
            set { Config.Current.MainView.IsTopmost = value; }
        }
    }
}
