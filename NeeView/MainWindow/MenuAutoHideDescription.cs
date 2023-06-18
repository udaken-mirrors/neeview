using System;
using System.Windows;
using System.Windows.Input;


namespace NeeView
{
    public class MenuAutoHideDescription : BasicAutoHideDescription
    {
        private readonly MenuBar _menuBar;
        private readonly SidePanelFrameView _sidePanelFrame;

        public MenuAutoHideDescription(FrameworkElement target, MenuBar menuBar, SidePanelFrameView sidePanelFrame) : base(target)
        {
            if (sidePanelFrame is null) throw new ArgumentNullException(nameof(sidePanelFrame));

            _menuBar = menuBar;
            _sidePanelFrame = sidePanelFrame;
        }

        public override bool IsIgnoreMouseOverAppendix()
        {
            if (!_sidePanelFrame.IsPanelMouseOver())
            {
                return false;
            }

            switch (Config.Current.AutoHide.AutoHideConfrictTopMargin)
            {
                default:
                case AutoHideConfrictMode.Allow:
                    return false;

                case AutoHideConfrictMode.AllowPixel:
                    var pos = Mouse.GetPosition(_sidePanelFrame);
                    return pos.Y > 1.5;

                case AutoHideConfrictMode.Deny:
                    return true;
            }
        }

        public override bool IsVisibleLocked()
        {
            if (_menuBar.IsMaximizeButtonMouseOver)
            {
                return true;
            }

            return base.IsVisibleLocked();
        }
    }
}
