using NeeView.Runtime.LayoutPanel;

namespace NeeView
{
    public class LayoutPanelAlternativeContent : IDisposableContent
    {
        private readonly CustomLayoutPanelManager _layoutPanelManager;
        private readonly SeparateLayoutPanel _separateLayoutPanel;
        private readonly LayoutPanel _layoutPanel;

        public LayoutPanelAlternativeContent(string panelName, bool store)
        {
            _layoutPanelManager = CustomLayoutPanelManager.Current;
            _layoutPanel = _layoutPanelManager.Panels[panelName];

            if (store)
            {
                _layoutPanelManager.AlternativePanelSource = AlternativePanelSource.Create(_layoutPanelManager, _layoutPanel);
            }

            _layoutPanelManager.SetSeparate(_layoutPanel, true);

            _separateLayoutPanel = new SeparateLayoutPanel();
            _separateLayoutPanel.LayoutPanel = _layoutPanel;
        }

        public object? Content => _separateLayoutPanel;

        public void Dispose()
        {
            _separateLayoutPanel.LayoutPanel = null;
            _layoutPanelManager.SetSeparate(_layoutPanel, false);

            _layoutPanelManager.AlternativePanelSource?.Restore(_layoutPanelManager, _layoutPanel);
            _layoutPanelManager.AlternativePanelSource = null;
        }
    }
}
