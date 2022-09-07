namespace NeeView
{
    public class EffectPanelAccessor : LayoutPanelAccessor
    {
        private readonly ImageEffectPanel _panel;


        public EffectPanelAccessor() : base(nameof(ImageEffectPanel))
        {
            _panel = (ImageEffectPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(ImageEffectPanel));
        }


        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }

}
