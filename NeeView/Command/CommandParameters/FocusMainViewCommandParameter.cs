using NeeView.Windows.Property;

namespace NeeView
{
    public class FocusMainViewCommandParameter : CommandParameter
    {
        private bool _needClosePanels;

        [PropertyMember]
        public bool NeedClosePanels
        {
            get => _needClosePanels;
            set => SetProperty(ref _needClosePanels, value);
        }
    }

}
