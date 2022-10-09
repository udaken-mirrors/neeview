namespace NeeView.Runtime.LayoutPanel
{
    /// <summary>
    /// LayoutDockPanel の１パネルを構成する要素
    /// </summary>
    public class LayoutDockPanelNode
    {
        public LayoutDockPanelNode(LayoutDockPanelContent dock, LayoutPanelCollection panels)
        {
            //if (!dock.Contains(panels)) throw new ArgumentException("dock not contains panels");
            Dock = dock;
            Panels = panels;
        }

        /// <summary>
        /// Collectionが所属するDock
        /// </summary>
        public LayoutDockPanelContent Dock { get; }

        /// <summary>
        /// １パネルを構成する要素
        /// </summary>
        public LayoutPanelCollection Panels { get; }
    }

}
