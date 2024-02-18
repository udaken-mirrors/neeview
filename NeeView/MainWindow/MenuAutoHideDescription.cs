using System;
using System.Windows;
using System.Windows.Input;


namespace NeeView
{
    public class MenuAutoHideDescription : BasicAutoHideDescription
    {
        private readonly SidePanelFrameView _sidePanelFrame;
        private MenuBar? _menuBar;

        public MenuAutoHideDescription(FrameworkElement target, SidePanelFrameView sidePanelFrame) : base(target)
        {
            if (sidePanelFrame is null) throw new ArgumentNullException(nameof(sidePanelFrame));

            _sidePanelFrame = sidePanelFrame;
        }


        public void SetMenuBar(MenuBar menuBar)
        {
            _menuBar = menuBar;
        }

        public override bool IsIgnoreMouseOverAppendix()
        {
            if (!_sidePanelFrame.IsPanelMouseOver())
            {
                return false;
            }

            switch (Config.Current.AutoHide.AutoHideConflictTopMargin)
            {
                default:
                case AutoHideConflictMode.Allow:
                    return false;

                case AutoHideConflictMode.AllowPixel:
                    var pos = Mouse.GetPosition(_sidePanelFrame);
                    return pos.Y > 1.5;

                case AutoHideConflictMode.Deny:
                    return true;
            }
        }

        public override bool IsVisibleLocked()
        {
            if (_menuBar is not null && _menuBar.IsMaximizeButtonMouseOver)
            {
                return true;
            }

            return base.IsVisibleLocked();
        }
    }
}
