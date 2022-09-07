namespace NeeView
{
    public class InformationPanelAccessor : LayoutPanelAccessor
    {
        private readonly FileInformationPanel _panel;
        private readonly FileInformation _model;


        public InformationPanelAccessor() : base(nameof(FileInformationPanel))
        {
            _panel = (FileInformationPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(FileInformationPanel));
            _model = _panel.FileInformation;
        }


        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }

}
